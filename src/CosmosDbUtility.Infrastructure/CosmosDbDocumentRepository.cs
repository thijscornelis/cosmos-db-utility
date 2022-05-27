using CosmosDbUtility.GUI.Abstractions;
using Microsoft.Azure.Cosmos;
using System.Runtime.CompilerServices;

namespace CosmosDbUtility.Infrastructure;

public class CosmosDbDocumentRepository : IDocumentRepository
{
	 private readonly CosmosClient _cosmosClient;

	 public CosmosDbDocumentRepository(CosmosClient cosmosClient) => _cosmosClient = cosmosClient;

	 /// <inheritdoc />
	 public async IAsyncEnumerable<string> GetContainerIdentifiersAsync(string databaseId, [EnumeratorCancellation] CancellationToken cancellationToken)
	 {
		  var database = _cosmosClient.GetDatabase(databaseId);
		  var iterator = database.GetContainerQueryIterator<Container>();
		  while (iterator.HasMoreResults)
		  {
				var response = await iterator.ReadNextAsync(cancellationToken);
				foreach (var container in response.Resource)
				{
					 yield return container.Id;
				}
		  }
	 }

	 /// <inheritdoc />
	 public async IAsyncEnumerable<string> GetDatabaseIdentifiersAsync([EnumeratorCancellation] CancellationToken cancellationToken)
	 {
		  var iterator = _cosmosClient.GetDatabaseQueryIterator<Database>();
		  while (iterator.HasMoreResults)
		  {
				var response = await iterator.ReadNextAsync(cancellationToken);
				foreach (var item in response.Resource)
				{
					 yield return item.Id;
				}
		  }
	 }
}
