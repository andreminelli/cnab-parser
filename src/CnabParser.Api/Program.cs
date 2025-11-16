using CnabParser.Application.Services;
using CnabParser.Core;
using CnabParser.Core.Repositories;
using CnabParser.Infrastructure;
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

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler(exceptionHandlerApp
    => exceptionHandlerApp.Run(async context
        => await Results.Problem().ExecuteAsync(context)));

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/docs");
}

app.UseHttpsRedirection();
app.MapControllers();

// Create database on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CnabDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.Run();

public partial class Program { }
