using Newtonsoft.Json.Linq;

namespace CosmosDbUtility.API;

public interface IFileService
{
	public IAsyncEnumerable<JObject> ReadFromAsync(string directory, CancellationToken cancellationToken);

	public Task WriteToAsync(string directory, int order, JObject document, CancellationToken cancellationToken);
}