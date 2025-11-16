using CnabParser.Core.Entities;
using Shouldly;
using Xunit;

namespace CnabParser.Core.Tests.Entities;

public class TransactionTests
{
    private readonly Store _testStore = new() { Name = "Test Store", OwnerName = "John Doe" };
   
    [Fact]
    public void GetSignedAmount_WithZeroAmount_ShouldReturnZero()
    {
        // Arrange
        var transaction = new Transaction
        {
            Store = _testStore,
            Type = TransactionType.Credit,
            Amount = 0m
        };

        // Act
        var signedAmount = transaction.GetSignedAmount();

        // Assert
        signedAmount.ShouldBe(0m);
    }

    [Theory]
    [InlineData(TransactionType.Debit,        1)]
    [InlineData(TransactionType.Boleto,      -1)]
    [InlineData(TransactionType.Financing,   -1)]
    [InlineData(TransactionType.Credit,       1)]
    [InlineData(TransactionType.LoanReceipt,  1)]
    [InlineData(TransactionType.Sales,        1)]
    [InlineData(TransactionType.TedReceipt,   1)]
    [InlineData(TransactionType.DocReceipt,   1)]
    [InlineData(TransactionType.Rent,        -1)]
    public void GetSignedAmount_AllTransactionTypes_ShouldApplyCorrectSign(
        TransactionType type, int expectedSign)
    {
        // Arrange
        var amount = (decimal) Random.Shared.Next(1, 10000);
        var transaction = new Transaction
        {
            Store = _testStore,
            Type = type,
            Amount = amount
        };

        // Act
        var signedAmount = transaction.GetSignedAmount();

        // Assert
        signedAmount.ShouldBe(amount * expectedSign);
    }
}
