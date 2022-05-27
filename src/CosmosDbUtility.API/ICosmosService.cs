namespace CosmosDbUtility.API;

public interface ICosmosService
{
	public Task Backup(string databaseId, string containerId, string directory, CancellationToken cancellationToken);

	public Task Restore(string databaseId, string containerId, string directory, CancellationToken cancellationToken);
}