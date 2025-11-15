using Microsoft.Extensions.Logging;

namespace CnabParser.Application;

public class CnabImporterService(
    ICnabFileParser cnabParser,
    ILogger<CnabImporterService> logger) : ICnabImporterService
{
    private readonly ICnabFileParser _cnabParser = cnabParser;
    private readonly ILogger<CnabImporterService> _logger = logger;

    public async Task<object> ImportAsync(Stream fileStream, CancellationToken cancellationToken)
    {
        await foreach (var transaction in _cnabParser.ParseAsync(fileStream))
        {
            _logger.LogInformation(System.Text.Json.JsonSerializer.Serialize(transaction));
        }

        return 0;
    }
}
