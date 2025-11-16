using CnabParser.Application.Helpers;
using CnabParser.Application.Services;
using CnabParser.Core.Entities;
using CnabParser.Core.Repositories;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using Xunit;

namespace CnabParser.Application.Tests.Services;

public class CnabImporterServiceTests
{
    private readonly ICnabFileParser _cnabParser = Substitute.For<ICnabFileParser>();
    private readonly ITransactionRepository _transactionRepository = Substitute.For<ITransactionRepository>();
    private readonly IStoreRepository _storeRepository = Substitute.For<IStoreRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ILogger<CnabImporterService> _logger = Substitute.For<ILogger<CnabImporterService>>();
    private readonly CnabImporterService _service;

    public CnabImporterServiceTests()
    {
        _service = new CnabImporterService(_cnabParser, _transactionRepository, _storeRepository, _unitOfWork, _logger);
    }

    #region ImportAsync Tests

    [Fact]
    public async Task ImportAsync_WithValidTransactions_ReturnsImportResponseWithCorrectCount()
    {
        // Arrange
        var fileStream = new MemoryStream();
        var store = new Store { Name = "Store 1", OwnerName = "Owner 1" };
        var transactions = new List<Transaction>
        {
            new() { Store = store, Amount = 100, Date = DateTimeOffset.Now, Type = TransactionType.Credit, Cpf = "12345678901", Card = "123456789012" },
            new() { Store = store, Amount = 50, Date = DateTimeOffset.Now, Type = TransactionType.Debit, Cpf = "12345678901", Card = "123456789012" },
            new() { Store = store, Amount = 75, Date = DateTimeOffset.Now, Type = TransactionType.Credit, Cpf = "12345678901", Card = "123456789012" }
        };

        _cnabParser.ParseAsync(fileStream).Returns(transactions.ToAsyncEnumerable());
        _storeRepository.GetOrCreateAsync(Arg.Any<Store>()).Returns(store);

        // Act
        var result = await _service.ImportAsync(fileStream, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.TransactionCount.ShouldBe(3);
        result.ImportId.ShouldNotBeNullOrEmpty();

        _ = _transactionRepository.Received(3).AddAsync(Arg.Any<Transaction>());
        _ = _transactionRepository.Received().AddAsync(Arg.Is<Transaction>(t => t.DataSourceId.ToString() == result.ImportId));
    }

    [Fact]
    public async Task ImportAsync_BeginTransactionAndCommitOnSuccess()
    {
        // Arrange
        var fileStream = new MemoryStream();
        var store = new Store { Name = "Store 1", OwnerName = "Owner 1" };
        var transactions = new[] { new Transaction { Store = store, Amount = 100, Cpf = "12345678901", Card = "123456789012" } };

        _cnabParser.ParseAsync(fileStream).Returns(transactions.ToAsyncEnumerable());
        _storeRepository.GetOrCreateAsync(Arg.Any<Store>()).Returns(store);

        // Act
        await _service.ImportAsync(fileStream, CancellationToken.None);

        // Assert
        Received.InOrder(async () =>
        {
            await _unitOfWork.BeginTransactionAsync();
            await _transactionRepository.AddAsync(Arg.Any<Transaction>());
            await _unitOfWork.CommitAsync();
        });
    }

    [Fact]
    public async Task ImportAsync_RollsBackOnException()
    {
        // Arrange
        var fileStream = new MemoryStream();
        var store = new Store { Name = "Store 1", OwnerName = "Owner 1" };
        var transactions = new[] { new Transaction { Store = store, Amount = 100, Cpf = "12345678901", Card = "123456789012" } };

        _cnabParser.ParseAsync(fileStream).Returns(transactions.ToAsyncEnumerable());
        _storeRepository.GetOrCreateAsync(Arg.Any<Store>()).Throws(new InvalidOperationException("Store error"));

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _service.ImportAsync(fileStream, CancellationToken.None));

        await _unitOfWork.Received(1).RollbackAsync();
        await _unitOfWork.DidNotReceive().CommitAsync();
    }

    #endregion

    #region GetImportDataAsync Tests

    [Fact]
    public async Task GetImportDataAsync_ReturnsGroupedTransactionsByStore()
    {
        // Arrange
        var importId = Guid.NewGuid();
        var store1 = new Store { Name = "Store 1", OwnerName = "Owner 1" };
        var store2 = new Store { Name = "Store 2", OwnerName = "Owner 2" };

        var transactions = new List<Transaction>
        {
            new() { Store = store1, Amount = 100, DataSourceId = importId, Cpf = "12345678901", Card = "123456789012" },
            new() { Store = store1, Amount = 50, DataSourceId = importId, Cpf = "12345678901", Card = "123456789012" },
            new() { Store = store2, Amount = 75, DataSourceId = importId, Cpf = "12345678901", Card = "123456789012" }
        };

        _transactionRepository.GetByDataSourceIdAsync(importId, CancellationToken.None)
            .Returns(transactions.ToAsyncEnumerable());

        // Act
        var result = await _service.GetImportDataAsync(importId, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Data.ShouldNotBeEmpty();
        result.Data.Count.ShouldBe(2);
        result.Data[0].Transactions.Length.ShouldBe(2);
        result.Data[1].Transactions.Length.ShouldBe(1);
    }

    [Fact]
    public async Task GetImportDataAsync_WithNoTransactions_ReturnsEmptyData()
    {
        // Arrange
        var importId = Guid.NewGuid();
        var emptyTransactions = new List<Transaction>();

        _transactionRepository.GetByDataSourceIdAsync(importId, CancellationToken.None)
            .Returns(emptyTransactions.ToAsyncEnumerable());

        // Act
        var result = await _service.GetImportDataAsync(importId, CancellationToken.None);

        // Assert
        result.Data.ShouldBeEmpty();
    }

    #endregion
}
