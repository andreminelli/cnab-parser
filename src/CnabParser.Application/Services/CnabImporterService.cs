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

    public async Task<object> ImportAsync(Stream fileStream, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();

        var count = 0;
        try
        {
            await foreach (var transaction in _cnabParser.ParseAsync(fileStream))
            {
                //_logger.LogInformation(System.Text.Json.JsonSerializer.Serialize(transaction));
                
                transaction.Store = await _storeRepository.GetOrCreateAsync(transaction.Store);
                await _transactionRepository.AddAsync(transaction);

                count++;
            }

            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }

        return count;
    }
}
