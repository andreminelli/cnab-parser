using CnabParser.Application.Dtos;
using CnabParser.Core.Entities;
using CnabParser.Core.Repositories;
using CnabParser.Core.Services;

namespace CnabParser.Application.UseCases;

public interface IImportCnabFileUseCase
{
    Task ExecuteAsync(Stream fileStream);
}

/// <summary>
/// Use case for importing CNAB files.
/// Handles parsing, store creation, and transaction persistence.
/// </summary>
public class ImportCnabFileUseCase : IImportCnabFileUseCase
{
    private readonly ICnabParserService _cnabParserService;
    private readonly IStoreRepository _storeRepository;
    private readonly ITransactionRepository _transactionRepository;

    public ImportCnabFileUseCase(
        ICnabParserService cnabParserService,
        IStoreRepository storeRepository,
        ITransactionRepository transactionRepository)
    {
        _cnabParserService = cnabParserService;
        _storeRepository = storeRepository;
        _transactionRepository = transactionRepository;
    }

    /// <summary>
    /// Executes the import process:
    /// 1. Parses CNAB file
    /// 2. Groups transactions by store
    /// 3. Creates or retrieves stores
    /// 4. Persists transactions
    /// </summary>
    public async Task ExecuteAsync(Stream fileStream)
    {
        // Parse all transactions from CNAB file
        var transactions = await _cnabParserService.ParseCnabFileAsync(fileStream);

        // Group transactions by store (name and owner)
        var groupedByStore = transactions.GroupBy(t => new { t.StoreName, t.StoreOwner });

        foreach (var storeGroup in groupedByStore)
        {
            var storeName = storeGroup.Key.StoreName;
            var storeOwner = storeGroup.Key.StoreOwner;

            // Get or create store
            var store = await _storeRepository.GetByNameAndOwnerAsync(storeName, storeOwner);
            if (store == null)
            {
                store = new Store
                {
                    Name = storeName,
                    Owner = storeOwner
                };
                await _storeRepository.AddAsync(store);
            }

            // Add transactions to database
            await _transactionRepository.AddRangeAsync(storeGroup.ToList());
        }
    }
}
