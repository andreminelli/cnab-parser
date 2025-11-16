namespace CnabParser.Application.Services;

public interface ICnabImporterService
{
    Task<ImportDataResponse> GetImportDataAsync(Guid dataSourceId, CancellationToken cancellationToken);
    Task<ImportResponse> ImportAsync(Stream fileStream, CancellationToken cancellationToken);
}
