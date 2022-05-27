// See https://aka.ms/new-console-template for more information

using Autofac;
using CosmosDbUtility.Abstractions;
using CosmosDbUtility.Application;
using CosmosDbUtility.GUI.Abstractions;
using CosmosDbUtility.Infrastructure;
using Microsoft.Azure.Cosmos;

Console.WriteLine("Hello, World!");
var container = SetupDependencyInjection();
using var scope = container.BeginLifetimeScope();

var facade = scope.Resolve<IFacade>();
await facade.BackupAsync(CancellationToken.None);
Console.ReadLine();

IContainer SetupDependencyInjection()
{
	 var builder = new ContainerBuilder();
	 builder.RegisterType<CosmosDbDocumentRepository>().As<IDocumentRepository>();
	 builder.RegisterType<FileRepository>().As<IFileRepository>();
	 builder.RegisterType<Facade>().As<IFacade>(); builder.Register((_) => new CosmosClient("https://localhost:8081", "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==", new CosmosClientOptions { ConnectionMode = ConnectionMode.Gateway }));
	 return builder.Build();
}
