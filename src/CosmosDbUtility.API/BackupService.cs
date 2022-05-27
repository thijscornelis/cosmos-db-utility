using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

namespace CosmosDbUtility.API;

public interface ICosmosService
{
	 public Task Backup(string databaseId, string containerId, string directory, CancellationToken cancellationToken);

	 public Task Restore(string databaseId, string containerId, string directory, CancellationToken cancellationToken);
}

public interface IFileService
{
	 public IAsyncEnumerable<JObject> ReadFromAsync(string directory, CancellationToken cancellationToken);

	 public Task WriteToAsync(string directory, int order, JObject document, CancellationToken cancellationToken);
}

public class CosmosService : ICosmosService
{
	 private readonly CosmosClient _cosmosClient;
	 private readonly IFileService _fileService;
	 private readonly ILogger<CosmosService> _logger;

	 public CosmosService(CosmosClient cosmosClient, ILogger<CosmosService> logger, IFileService fileService)
	 {
		  _cosmosClient = cosmosClient;
		  _logger = logger;
		  _fileService = fileService;
	 }

	 /// <inheritdoc />
	 public async Task Backup(string databaseId, string containerId, string directory, CancellationToken cancellationToken)
	 {
		  var counter = 0;
		  var database = GetDatabase(databaseId);
		  var container = GetContainer(database, containerId);
		  var iterator = GetDocumentsIterator(container, cancellationToken);
		  await foreach (var document in iterator.WithCancellation(cancellationToken))
		  {
				if (document is not JObject o)
					 throw new Exception("Expected document to be a valid JObject");
				counter++;
				_logger.LogInformation(@"Found document with id {0}", o);
				await _fileService.WriteToAsync(directory, counter, o, cancellationToken);
		  }
	 }

	 /// <inheritdoc />
	 public async Task Restore(string databaseId, string containerId, string directory, CancellationToken cancellationToken)
	 {
		  var database = GetDatabase(databaseId);
		  var container = GetContainer(database, containerId);
		  var iterator = _fileService.ReadFromAsync(directory, cancellationToken);
		  await foreach (JObject o in iterator.WithCancellation(cancellationToken))
		  {
				await container.CreateItemAsync(o, cancellationToken: cancellationToken);
		  }
	 }

	 private Container GetContainer(Database db, string id) => db.GetContainer(id);

	 private Database GetDatabase(string id) => _cosmosClient.GetDatabase(id);

	 private async IAsyncEnumerable<object> GetDocumentsIterator(Container container, [EnumeratorCancellation] CancellationToken cancellationToken)
	 {
		  var feedIterator = container.GetItemLinqQueryable<object>().ToFeedIterator();
		  while (feedIterator.HasMoreResults)
		  {
				var response = await feedIterator.ReadNextAsync(cancellationToken);
				foreach (var documents in response.Resource)
				{
					 yield return documents;
				}
		  }
	 }
}

public class FileService : IFileService
{
	 /// <inheritdoc />
	 public async IAsyncEnumerable<JObject> ReadFromAsync(string directory, [EnumeratorCancellation] CancellationToken cancellationToken)
	 {
		  ThrowIfDirectoryDoesNotExist(directory);
		  var enumerable = Directory.EnumerateFiles(directory, "*.json").OrderBy(x => x);
		  foreach (var item in enumerable)
		  {
				using StreamReader file = File.OpenText(item);
				using JsonTextReader reader = new JsonTextReader(file);

				yield return (JObject)(await JToken.ReadFromAsync(reader, cancellationToken));
		  }
	 }

	 /// <inheritdoc />
	 public async Task WriteToAsync(string directory, int order, JObject document, CancellationToken cancellationToken)
	 {
		  ThrowIfDirectoryDoesNotExist(directory);
		  var documentId = ThrowIfDocumentIdNotFound(document);
		  await using StreamWriter file = File.CreateText(Path.Combine(directory, $"{order:00000000}-{documentId}.json"));
		  using JsonTextWriter writer = new JsonTextWriter(file);
		  await document.WriteToAsync(writer, cancellationToken);
	 }

	 private static string ThrowIfDocumentIdNotFound(JObject document)
	 {
		  if (document == null) throw new ArgumentNullException(nameof(document));
		  var documentId = document["id"];
		  if (documentId == null) throw new Exception("Document is missing an \"id\" property");
		  return documentId.ToString();
	 }

	 private void ThrowIfDirectoryDoesNotExist(string directory)
	 {
		  if (!Directory.Exists(directory)) throw new DirectoryNotFoundException($"Directory {directory} does not exist");
	 }
}

public class ChangeFeedProcessor : IHostedService
{
	 private readonly CosmosClient _cosmosClient;
	 private readonly ILogger<ChangeFeedProcessor> _logger;

	 /// <summary>Initializes a new instance of the <see cref="ChangeFeedProcessor" /> class.</summary>
	 /// <param name="cosmosClient">The cosmos client.</param>
	 /// <param name="logger">The logger.</param>
	 public ChangeFeedProcessor(CosmosClient cosmosClient, ILogger<ChangeFeedProcessor> logger)
	 {
		  _cosmosClient = cosmosClient;
		  _logger = logger;
	 }

	 /// <summary>Gets the ChangeFeed processor</summary>
	 protected Lazy<Microsoft.Azure.Cosmos.ChangeFeedProcessor> Processor { get; private set; }

	 /// <summary>Gets the name of the processor</summary>
	 protected string ProcessorName { get; } = "CosmosDbUtilityChangeFeedProcessor";

	 /// <inheritdoc />
	 public async Task StartAsync(CancellationToken cancellationToken)
	 {
		  Processor = new(() =>
		  {
				var db = _cosmosClient.GetDatabase("unify");
				return db.GetContainer("restore")
				  .GetChangeFeedProcessorBuilder<JObject>(ProcessorName, HandleChangesAsync)
				  .WithStartTime(DateTime.MinValue.ToUniversalTime())
				  .WithInstanceName($"PROCESSOR_{Environment.MachineName}_{Environment.ProcessId}")
				  .WithLeaseContainer(db.GetContainer("leases"))
				  .WithLeaseAcquireNotification(OnLeaseAcquired)
				  .WithLeaseReleaseNotification(OnLeaseReleased)
				  .WithErrorNotification(OnError)
				  .Build();
		  });

		  await Processor.Value.StartAsync();
		  _logger.LogInformation("{ProcessorName}: Processor STARTED", ProcessorName);
	 }

	 /// <inheritdoc />
	 public async Task StopAsync(CancellationToken cancellationToken)
	 {
		  await Processor.Value.StopAsync();
		  _logger.LogInformation("{ProcessorName}: Processor STOPPED", ProcessorName);
	 }

	 private Task HandleChangesAsync(ChangeFeedProcessorContext context, IReadOnlyCollection<JObject> changes, CancellationToken cancellationtoken)
	 {
		  _logger.LogInformation("{ProcessorName}: Started handling changes for lease {LeaseToken}...", ProcessorName, context.LeaseToken);
		  _logger.LogInformation("{ProcessorName}: Change Feed request consumed {RequestCharge} RU", ProcessorName, context.Headers.RequestCharge);
		  _logger.LogInformation("{ProcessorName}: SessionToken {SessionToken}", ProcessorName, context.Headers.Session);

		  foreach (var change in changes)
		  {
				_logger.LogInformation(change.ToString());
		  }

		  return Task.CompletedTask;
	 }

	 private Task OnError(string leasetoken, Exception exception)
	 {
		  _logger.LogError(exception, "{ProcessorName}: Lease {LeaseToken} ERROR, {ErrorMessage}", ProcessorName, leasetoken, exception.GetBaseException().Message);
		  return Task.CompletedTask;
	 }

	 private Task OnLeaseAcquired(string leasetoken)
	 {
		  _logger.LogInformation("{ProcessorName}: Lease {LeaseToken} ACQUIRED", ProcessorName, leasetoken);
		  return Task.CompletedTask;
	 }

	 private Task OnLeaseReleased(string leasetoken)
	 {
		  _logger.LogInformation("{ProcessorName}: Lease {LeaseToken} RELEASED", ProcessorName, leasetoken);
		  return Task.CompletedTask;
	 }
}
