using CnabParser.Application.Helpers;
using CnabParser.Core.Repositories;
using Microsoft.Extensions.Logging;

namespace CnabParser.Application.Services;

public class CnabImporterService(
    ICnabFileParser cnabParser,
    ITransactionRepository transactionRepository,
    IStoreRepository storeRepository,
    IUnitOfWork unitOfWork,
    ILogger<CnabImporterService> logger) : ICnabImporterService
{
    private readonly ICnabFileParser _cnabParser = cnabParser;
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly IStoreRepository _storeRepository = storeRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<CnabImporterService> _logger = logger;

    public async Task<ImportDataResponse> GetImportDataAsync(Guid importId, CancellationToken cancellationToken)
    {
        var transactions = _transactionRepository.GetByDataSourceIdAsync(importId, cancellationToken);

        var result = await transactions
            .GroupBy(t => t.Store)
            .Select(g => new StoreResponse(g.Key, g))
            .ToListAsync();

        return new ImportDataResponse(result);
    }

    public async Task<ImportResponse> ImportAsync(Stream fileStream, CancellationToken cancellationToken)
    {
        var dataSourceId = Guid.CreateVersion7();

        await _unitOfWork.BeginTransactionAsync();

        var count = 0;
        try
        {
            await foreach (var transaction in _cnabParser.ParseAsync(fileStream))
            {
                cancellationToken.ThrowIfCancellationRequested();

                transaction.Store = await _storeRepository.GetOrCreateAsync(transaction.Store);
                transaction.DataSourceId = dataSourceId;
                await _transactionRepository.AddAsync(transaction);

                count++;
            }

            await _unitOfWork.CommitAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed importing file");
            await _unitOfWork.RollbackAsync();
            throw;
        }

        return new ImportResponse(dataSourceId.ToString(), count);
    }
}
