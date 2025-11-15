using CnabParser.Application;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ICnabFileParser, CnabFileParser>();

builder.Services.AddScoped<ICnabImporterService, CnabImporterService>();

var app = builder.Build();

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
        return TypedResults.Ok();
    })
    .DisableAntiforgery()
    .WithOpenApi(operation => new(operation)
    {
        Summary = "CNAB Files",
        Description = "Upload a CNAB file"
    });

app.Run();
