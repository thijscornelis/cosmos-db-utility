using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;

namespace CosmosDbUtility.API;

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