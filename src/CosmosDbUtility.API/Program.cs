using CosmosDbUtility.API;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container. Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton(_ => new CosmosClient("https://localhost:8081", "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==", new CosmosClientOptions { ConnectionMode = ConnectionMode.Gateway }));
builder.Services.AddTransient<ICosmosService, CosmosService>();
builder.Services.AddTransient<IFileService, FileService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	 app.UseSwagger();
	 app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/api/backup/unify", async (string directory, ICosmosService service, CancellationToken cancellationToken) =>
{
	 await service.Backup("unify", "events", Path.Combine(directory, "events"), cancellationToken);
	 await service.Backup("unify", "snapshots", Path.Combine(directory, "snapshots"), cancellationToken);
});

app.MapPost("/api/restore/unify", async (string directory, ICosmosService service, CancellationToken cancellationToken) =>
{
	 await service.Restore("unify", "events", Path.Combine(directory, "events"), cancellationToken);
	 await service.Restore("unify", "snapshots", Path.Combine(directory, "snapshots"), cancellationToken);
});

app.Run();
