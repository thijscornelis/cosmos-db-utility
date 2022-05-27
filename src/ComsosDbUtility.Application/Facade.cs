using CosmosDbUtility.Abstractions;
using CosmosDbUtility.GUI.Abstractions;

namespace CosmosDbUtility.Application;

public class Facade : IFacade
{
	 private IDocumentRepository _documentRepository;

	 public Facade(IDocumentRepository documentRepository) => _documentRepository = documentRepository;

	 /// <inheritdoc />
	 public async Task BackupAsync(CancellationToken cancellationToken)
	 {
		  var databaseId = await SelectDatabase(cancellationToken);
		  throw new NotImplementedException();
	 }

	 private async Task<string[]> GetDatabases(CancellationToken cancellationToken)
	 {
		  var result = new List<string>();
		  var enumerable = _documentRepository.GetDatabaseIdentifiersAsync(cancellationToken);
		  await foreach (var database in enumerable.WithCancellation(cancellationToken))
		  {
				result.Add(database);
		  }
		  return result.ToArray();
	 }

	 private async Task<string> SelectDatabase(CancellationToken cancellationToken)
	 {
		  Console.WriteLine("Which database do you want to select?");

		  var counter = 0;
		  var databases = await GetDatabases(cancellationToken);
		  foreach (var database in databases)
		  {
				Console.WriteLine(@"{0}. {1}", ++counter, database);
		  }

		  Console.WriteLine("Confirm your selection with \"Enter\"...");
		  var input = Console.ReadLine();

		  if (!int.TryParse(input, out var index))
		  {
				Console.WriteLine("Invalid input, try again");
				await SelectDatabase(cancellationToken);
		  }

		  Console.WriteLine(@"You chose to backup from {0}", databases[index]);
		  return databases[index];
	 }
}
