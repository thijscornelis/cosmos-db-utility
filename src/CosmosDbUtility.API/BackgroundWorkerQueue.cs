using System.Collections.Concurrent;

public class BackgroundWorkerQueue
{
	private readonly List<BackgroundWorkerJob> _dequeuedJobs = new();
	private readonly ConcurrentQueue<BackgroundWorkerJob> _jobs = new();
	private readonly SemaphoreSlim _signal = new(0);

	public IEnumerable<BackgroundWorkerJob> Jobs
	{
		get
		{
			var result = new List<BackgroundWorkerJob>(_jobs);
			result.AddRange(_dequeuedJobs);
			return result;
		}
	}

	public void Queue(BackgroundWorkerJob job)
	{
		_jobs.Enqueue(job);
		_signal.Release();
	}

	public async Task<BackgroundWorkerJob?> TryDequeueAsync(CancellationToken cancellationToken)
	{
		await _signal.WaitAsync(cancellationToken);
		if (_jobs.TryDequeue(out var job))
		{
			_dequeuedJobs.Add(job);
			return job;
		}

		return null;
	}
}