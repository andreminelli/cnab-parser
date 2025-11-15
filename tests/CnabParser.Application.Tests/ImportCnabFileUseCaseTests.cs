using CnabParser.Application.UseCases;
using CnabParser.Core.Entities;
using CnabParser.Core.Repositories;
using CnabParser.Core.Services;
using Xunit;

namespace CnabParser.Application.Tests;

public class ImportCnabFileUseCaseTests
{
    private class MockCnabParser : ICnabParserService
    {
        public Task<IEnumerable<Transaction>> ParseCnabFileAsync(Stream fileStream)
        {
            var transactions = new List<Transaction>
            {
                new Transaction
                {
                    Date = DateTime.Now,
                    Type = TransactionType.Debit,
                    Amount = 100m,
                    Cpf = "12345678901",
                    Card = "****1234",
                    Time = "120000",
                    StoreOwner = "Test Owner",
                    StoreName = "Test Store"
                }
            };

            return Task.FromResult(transactions.AsEnumerable());
        }
    }

    private class MockStoreRepository : IStoreRepository
    {
        private readonly List<Store> _stores = new();

        public Task<IEnumerable<Store>> GetAllAsync() => Task.FromResult(_stores.AsEnumerable());
        public Task<Store?> GetByIdAsync(int id) => Task.FromResult(_stores.FirstOrDefault(s => s.Id == id));
        public Task<Store?> GetByNameAndOwnerAsync(string name, string owner) 
            => Task.FromResult(_stores.FirstOrDefault(s => s.Name == name && s.Owner == owner));

        public Task AddAsync(Store store)
        {
            store.Id = _stores.Count + 1;
            _stores.Add(store);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Store store) => Task.CompletedTask;
    }

    private class MockTransactionRepository : ITransactionRepository
    {
        private readonly List<Transaction> _transactions = new();

        public Task AddAsync(Transaction transaction)
        {
            transaction.Id = _transactions.Count + 1;
            _transactions.Add(transaction);
            return Task.CompletedTask;
        }

        public Task AddRangeAsync(IEnumerable<Transaction> transactions)
        {
            foreach (var t in transactions)
            {
                t.Id = _transactions.Count + 1;
                _transactions.Add(t);
            }
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task ShouldImportCnabFileSuccessfully()
    {
        // Arrange
        var parser = new MockCnabParser();
        var storeRepo = new MockStoreRepository();
        var transactionRepo = new MockTransactionRepository();

        var useCase = new ImportCnabFileUseCase(parser, storeRepo, transactionRepo);
        var stream = new MemoryStream();

        // Act
        await useCase.ExecuteAsync(stream);

        // Assert
        var stores = await storeRepo.GetAllAsync();
        Assert.NotEmpty(stores);
        Assert.Single(stores);
    }
}
