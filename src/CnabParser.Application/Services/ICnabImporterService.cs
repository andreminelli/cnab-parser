namespace CnabParser.Application.Services;

public interface ICnabImporterService
{
    Task<object> ImportAsync(Stream fileStream, CancellationToken cancellationToken);
}
