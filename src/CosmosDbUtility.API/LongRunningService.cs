public class LongRunningService : BackgroundService
{
	private readonly BackgroundWorkerQueue _queue;

	public LongRunningService(BackgroundWorkerQueue queue)
	{
		_queue = queue;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			var job = await _queue.TryDequeueAsync(stoppingToken);
			if (job != null) await job.ExecuteAsync(stoppingToken);
		}
	}
}