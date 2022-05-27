namespace CosmosDbUtility.GUI.Abstractions;

public interface IDocumentRepository
{
	 IAsyncEnumerable<string> GetContainerIdentifiersAsync(string databaseId, CancellationToken cancellationToken);

	 IAsyncEnumerable<string> GetDatabaseIdentifiersAsync(CancellationToken cancellationToken);
}
