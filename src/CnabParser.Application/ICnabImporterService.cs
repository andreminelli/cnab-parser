namespace CnabParser.Application;

public interface ICnabImporterService
{
    Task<object> ImportAsync(Stream fileStream, CancellationToken cancellationToken);
}
