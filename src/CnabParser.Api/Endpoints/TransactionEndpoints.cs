using CnabParser.Application.UseCases;

namespace CnabParser.Api.Endpoints;

public static class TransactionEndpoints
{
    public static void MapTransactionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/transactions")
            .WithName("Transactions");

        group.MapPost("/import", ImportTransactions)
            .WithName("Import CNAB Transactions")
            .WithDescription("Upload and import a CNAB file containing transactions")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> ImportTransactions(
        IFormFile file,
        IImportCnabFileUseCase importUseCase)
    {
        if (file == null || file.Length == 0)
            return Results.BadRequest(new { error = "File is required" });

        if (!file.FileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            return Results.BadRequest(new { error = "Only .txt files are supported" });

        try
        {
            using var stream = file.OpenReadStream();
            await importUseCase.ExecuteAsync(stream);
            return Results.Ok(new { message = "Transactions imported successfully" });
        }
        catch (Exception ex)
        {
            return Results.Json(
                new { error = "An error occurred while importing transactions", detail = ex.Message },
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
