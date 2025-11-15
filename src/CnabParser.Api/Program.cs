using CnabParser.Api.Endpoints;
using CnabParser.Application.UseCases;
using CnabParser.Core.Repositories;
using CnabParser.Core.Services;
using CnabParser.Infrastructure.Data;
using CnabParser.Infrastructure.Repositories;
using CnabParser.Infrastructure.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults
builder.AddServiceDefaults();

// Add Database - Get connection string from Aspire service discovery
var connectionName = "cnabdb";
var connectionString = builder.Configuration.GetConnectionString(connectionName);

builder.Services.AddDbContext<CnabParserDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add Application Services
builder.Services.AddScoped<IImportCnabFileUseCase, ImportCnabFileUseCase>();
builder.Services.AddScoped<IGetStoresBalanceUseCase, GetStoresBalanceUseCase>();

// Add Infrastructure Services
builder.Services.AddScoped<ICnabParserService, CnabParserService>();
builder.Services.AddScoped<IStoreRepository, StoreRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

var app = builder.Build();

// Add Aspire service defaults middleware
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseReDoc(c =>
    {
        c.DocumentTitle = "REDOC API Documentation";
        c.SpecUrl = "/swagger/v1/swagger.json";
    });
}

app.UseHttpsRedirection();

// Map endpoints
app.MapTransactionEndpoints();

// Create database on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CnabParserDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.Run();
