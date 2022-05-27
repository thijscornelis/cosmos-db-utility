using System.Net;
using CosmosDbUtility.API;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container. Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton(_ => new CosmosClient("https://localhost:8081",
	"C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
	new CosmosClientOptions {ConnectionMode = ConnectionMode.Gateway}));
builder.Services.AddTransient<ICosmosService, CosmosService>();
builder.Services.AddTransient<IFileService, FileService>();
builder.Services.AddHostedService<LongRunningService>();
builder.Services.AddSingleton<BackgroundWorkerQueue>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/api/backup/unify", (string directory, BackgroundWorkerQueue worker, ICosmosService service) =>
{
	worker.Queue(new BackgroundWorkerJob("Backup Unify Events",
		async token => await service.Backup("unify", "events", Path.Combine(directory, "events"), token)));
	worker.Queue(new BackgroundWorkerJob("Backup Unify Snapshots",
		async token => await service.Backup("unify", "snapshots", Path.Combine(directory, "snapshots"), token)));
	return Results.Json(worker.Jobs.ToArray(), statusCode: (int) HttpStatusCode.Accepted,
		contentType: "application/json");
}).Produces((int) HttpStatusCode.Accepted, typeof(BackgroundWorkerJob[]), "application/json");

app.MapPost("/api/restore/unify", (string directory, BackgroundWorkerQueue worker, ICosmosService service) =>
{
	worker.Queue(new BackgroundWorkerJob("Restore Unify Events",
		async token => await service.Restore("unify", "events", Path.Combine(directory, "events"), token)));
	worker.Queue(new BackgroundWorkerJob("Restore Unify Snapshots",
		async token => await service.Restore("unify", "snapshots", Path.Combine(directory, "snapshots"), token)));
	return Results.Json(worker.Jobs.ToArray(), statusCode: (int) HttpStatusCode.Accepted,
		contentType: "application/json");
}).Produces((int) HttpStatusCode.Accepted, typeof(BackgroundWorkerJob[]), "application/json");
;

app.MapGet("api/jobs/{id}", (Guid id, BackgroundWorkerQueue worker) =>
{
	var task = worker.Jobs.Where(x => x.Id.Equals(id));
	return Results.Json(task);
});
app.MapGet("api/jobs", (BackgroundWorkerQueue worker) => Results.Json(worker.Jobs.ToList()));

app.Run();