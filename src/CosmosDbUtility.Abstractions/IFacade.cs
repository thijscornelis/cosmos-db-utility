namespace CosmosDbUtility.Abstractions;

public interface IFacade
{
	 Task BackupAsync(CancellationToken cancellationToken);
}
