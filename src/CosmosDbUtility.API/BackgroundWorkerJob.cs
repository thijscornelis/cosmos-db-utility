using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

public class BackgroundWorkerJob
{
	 public BackgroundWorkerJob(string name, Func<CancellationToken, Task> job)
	 {
		  ArgumentNullException.ThrowIfNull(job);
		  ArgumentNullException.ThrowIfNull(name);
		  Id = Guid.NewGuid();
		  Name = name;
		  Job = job;
		  JobStatus = JobStatus.NotStarted;
	 }

	 public Guid Id { get; }

	 [Newtonsoft.Json.JsonIgnore]
	 [System.Text.Json.Serialization.JsonIgnore]
	 public Func<CancellationToken, Task> Job { get; }

	 [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
	 [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
	 public JobStatus JobStatus { get; private set; }

	 public string Name { get; }

	 public async Task ExecuteAsync(CancellationToken cancellationToken)
	 {
		  if (cancellationToken.IsCancellationRequested) throw new TaskCanceledException();
		  try
		  {
				JobStatus = JobStatus.Running;
				await Job(cancellationToken);
				JobStatus = JobStatus.Completed;
		  }
		  catch
		  {
				JobStatus = JobStatus.Failed;
		  }
	 }
}
