using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CosmosDbUtility.API;

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