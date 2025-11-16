using CnabParser.Core.Entities;
using Shouldly;
using Xunit;

namespace CnabParser.Core.Tests.Entities;

public class TransactionExtensionsTests
{
    private readonly Store _testStore = new() { Name = "Test Store", OwnerName = "John Doe" };
    private List<Transaction> _transactions = [];

    [Fact]
    public void GetBalance_WithEmptyCollection_ShouldReturnZero()
    {
        // Arrange
        _transactions = [];

        // Act
        var balance = _transactions.GetBalance();

        // Assert
        balance.ShouldBe(0m);
    }

    [Fact]
    public void GetBalance_WithSingleCreditTransaction_ShouldReturnPositiveAmount()
    {
        // Arrange
        _transactions =
        [
            new()
            {
                Store = _testStore,
                Type = TransactionType.Credit,
                Amount = 100m
            }
        ];

        // Act
        var balance = _transactions.GetBalance();

        // Assert
        balance.ShouldBe(100m);
    }

    [Fact]
    public void GetBalance_WithSingleDebitTransaction_ShouldReturnNegativeAmount()
    {
        // Arrange
        _transactions =
        [
            new()
            {
                Store = _testStore,
                Type = TransactionType.Boleto,
                Amount = 50m
            }
        ];

        // Act
        var balance = _transactions.GetBalance();

        // Assert
        balance.ShouldBe(-50m);
    }

    [Fact]
    public void GetBalance_WithMultipleIncomeTransactions_ShouldReturnPositiveBalance()
    {
        // Arrange
        _transactions =
        [
            new() { Store = _testStore, Type = TransactionType.Credit, Amount = 100m }, // +100
            new() { Store = _testStore, Type = TransactionType.Debit, Amount = 50m },   // +50
            new() { Store = _testStore, Type = TransactionType.Sales, Amount = 75m }    // +75
        ];

        // Act
        var balance = _transactions.GetBalance();

        // Assert
        balance.ShouldBe(225m);
    }

    [Fact]
    public void GetBalance_WithMixedIncomeAndExpenseTransactions_ShouldCalculateNetBalance()
    {
        // Arrange
        _transactions =
        [
            new() { Store = _testStore, Type = TransactionType.Credit, Amount = 1000m },     // +1000
            new() { Store = _testStore, Type = TransactionType.Boleto, Amount = 200m },      // -200
            new() { Store = _testStore, Type = TransactionType.Sales, Amount = 500m },       // +500
            new() { Store = _testStore, Type = TransactionType.Financing, Amount = 300m },   // -300
            new() { Store = _testStore, Type = TransactionType.LoanReceipt, Amount = 150m }  // +150
        ];

        // Act
        var balance = _transactions.GetBalance();

        // Assert
        balance.ShouldBe(1150m);
    }

    [Fact]
    public void GetBalance_WithAllExpenseTransactions_ShouldReturnNegativeBalance()
    {
        // Arrange
        _transactions =
        [
            new() { Store = _testStore, Type = TransactionType.Boleto, Amount = 100m },      // -100
            new() { Store = _testStore, Type = TransactionType.Financing, Amount = 200m },   // -200
            new() { Store = _testStore, Type = TransactionType.Rent, Amount = 150m }         // -150
        ];

        // Act
        var balance = _transactions.GetBalance();

        // Assert
        // -100 - 200 - 150 = -450
        balance.ShouldBe(-450m);
    }

    [Fact]
    public void GetBalance_WithZeroAmountTransactions_ShouldReturnZero()
    {
        // Arrange
        _transactions =
        [
            new() { Store = _testStore, Type = TransactionType.Credit, Amount = 0m },
            new() { Store = _testStore, Type = TransactionType.Boleto, Amount = 0m },
            new() { Store = _testStore, Type = TransactionType.Sales, Amount = 0m }
        ];

        // Act
        var balance = _transactions.GetBalance();

        // Assert
        balance.ShouldBe(0m);
    }
}
