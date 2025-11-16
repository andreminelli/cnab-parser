using CnabParser.Application.Helpers;
using CnabParser.Application.Services;
using CnabParser.Core.Repositories;
using CnabParser.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var connectionName = "cnabdb";
var connectionString = builder.Configuration.GetConnectionString(connectionName);
builder.Services.AddDbContext<CnabDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddSingleton<ICnabFileParser, CnabFileParser>();

builder.Services.AddScoped<ICnabImporterService, CnabImporterService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IStoreRepository, StoreRepository>();

builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler(exceptionHandlerApp
    => exceptionHandlerApp.Run(async context
        => await Results.Problem().ExecuteAsync(context)));

if (app.Environment.IsDevelopment())
{
    app.MapSwagger("/openapi/{documentName}.json");
    app.MapScalarApiReference("/docs");
}

app.UseHttpsRedirection();

app
    .MapPost("/cnab-files", async (IFormFile file, ICnabImporterService cnabImporterService, CancellationToken cancellationToken) =>
    {
        await using var fileStream = file.OpenReadStream();
        var result = await cnabImporterService.ImportAsync(fileStream, cancellationToken);
        return TypedResults.Ok(result);
    })
    .DisableAntiforgery()
    .WithOpenApi(operation => new(operation)
    {
        Summary = "CNAB Files",
        Description = "Upload a CNAB file"
    });

// Create database on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CnabDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.Run();
