using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

namespace CosmosDbUtility.API;

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
