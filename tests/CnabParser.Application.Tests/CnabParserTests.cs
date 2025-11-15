using CnabParser.Core.Entities;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using System.Text;
using Xunit;

namespace CnabParser.Application.Tests;

public class CnabParserTests
{
    private readonly ILogger<CnabFileParser> _logger = Substitute.For<ILogger<CnabFileParser>>();

    private readonly CnabFileParser _parser;

    public CnabParserTests()
    {
        _parser = new CnabFileParser(_logger);
    }

    [Fact]
    public async Task ParseFileAsync_WithValidCnabLine_ReturnsTransaction()
    {
        // Arrange
        var validLine = "1201903010000020000556418150631234****3324090002MARIA JOSEFINALOJA DO Ó - MATRIZ\n";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(validLine));

        // Act
        var transactions = await _parser.ParseAsync(stream).ToListAsync();

        // Assert
        transactions.Count.ShouldBe(1);
        var firstTransaction = transactions[0];
        firstTransaction.Type.ShouldBe(TransactionType.Debit);
        firstTransaction.Date.ShouldBe(new DateTimeOffset(2019, 03, 01, 09, 00, 02, TimeSpan.FromHours(-3)));
        firstTransaction.Amount.ShouldBe(200m);
        firstTransaction.Cpf.ShouldBe("55641815063");
        firstTransaction.Card.ShouldBe("1234****3324");
        var store = firstTransaction.Store.ShouldNotBeNull();
        store.Name.ShouldBe("LOJA DO Ó - MATRIZ");
        store.OwnerName.ShouldBe("MARIA JOSEFINA");
    }

    [Fact]
    public async Task ParseFileAsync_WithInvalidLineLength_SkipsLine()
    {
        // Arrange
        var invalidLine = "1202301151234567890\n"; // Too short
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(invalidLine));

        // Act
        var transactions = await _parser.ParseAsync(stream).ToListAsync();

        // Assert
        transactions.ShouldBeEmpty();
    }

    [Fact]
    public async Task ParseFileAsync_WithEmptyLines_SkipsEmptyLines()
    {
        // Arrange
        var validLine = "1201903010000020000556418150631234****3324090002MARIA JOSEFINALOJA DO Ó - MATRIZ\n";
        var content = $"\n\n{validLine}\n\n";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // Act
        var transactions = await _parser.ParseAsync(stream).ToListAsync();

        // Assert
        transactions.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task ParseFileAsync_WithMultipleValidLines_ReturnsAllTransactions()
    {
        // Arrange
        var line1 = "3201903010000014200096206760174753****3153153453JOÃO MACEDO   BAR DO JOÃO       \n";
        var line2 = "4201906010000050617845152540731234****2231100000MARCOS PEREIRAMERCADO DA AVENIDA";
        var content = line1 + line2;
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // Act
        var transactions = await _parser.ParseAsync(stream).ToListAsync();

        // Assert
        transactions.Count.ShouldBe(2);
        transactions[0].Type.ShouldBe(TransactionType.Financing);
        transactions[1].Type.ShouldBe(TransactionType.Credit);
    }

    [Theory]
    [InlineData('1', TransactionType.Debit)]
    [InlineData('2', TransactionType.Boleto)]
    [InlineData('3', TransactionType.Financing)]
    [InlineData('4', TransactionType.Credit)]
    [InlineData('5', TransactionType.LoanReceipt)]
    [InlineData('6', TransactionType.Sales)]
    [InlineData('7', TransactionType.TedReceipt)]
    [InlineData('8', TransactionType.DocReceipt)]
    [InlineData('9', TransactionType.Rent)]
    public async Task ParseFileAsync_WithDifferentTransactionTypes_ParsesCorrectType(char typeChar, TransactionType expectedType)
    {
        // Arrange
        var line = $"{typeChar}201903010000020000556418150631234****3324090002MARIA JOSEFINALOJA DO Ó - MATRIZ\n";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(line));

        // Act
        var transactions = await _parser.ParseAsync(stream).ToListAsync();

        // Assert
        transactions.ShouldHaveSingleItem();
        transactions[0].Type.ShouldBe(expectedType);
    }

    [Fact]
    public async Task ParseFileAsync_ParsesDateCorrectly()
    {
        // Arrange
        var line = "2201903210000010200232702980568473****1231231233JOSÉ COSTA    MERCEARIA 3 IRMÃOS\n";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(line));

        // Act
        var transactions = await _parser.ParseAsync(stream).ToListAsync();

        // Assert
        transactions.ShouldHaveSingleItem();
        transactions[0].Date.ShouldBe(new DateTimeOffset(2019, 03, 21, 23, 12, 33, TimeSpan.FromHours(-3)));
    }

    [Fact]
    public async Task ParseFileAsync_WithEmptyStream_ReturnsNoTransactions()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(""));

        // Act
        var transactions = await _parser.ParseAsync(stream).ToListAsync();

        // Assert
        transactions.ShouldBeEmpty();
    }
}
