using CnabParser.Core;
using CnabParser.Core.Entities;
using Microsoft.Extensions.Logging;

namespace CnabParser.Infrastructure;

public class CnabFileParser(ILogger<CnabFileParser> logger) : ICnabFileParser
{
    private const int CnabLineLength = 80;
    private static readonly TimeSpan DefaultTimezone = TimeSpan.FromHours(-3);

    private readonly ILogger<CnabFileParser> _logger = logger;

    public async IAsyncEnumerable<Transaction> ParseAsync(Stream fileStream)
    {
        string? line;
        int lineNumber = 0;

        using var reader = new StreamReader(fileStream);

        while ((line = await reader.ReadLineAsync()) != null)
        {
            lineNumber++;

            if (string.IsNullOrWhiteSpace(line))
            {
                _logger.LogWarning("Failed to read full line in CNAB file at line {LineNumber}", lineNumber);
                continue;
            }

            Transaction? transaction = null;

            try
            {
                transaction = ParseCnabLine(line.AsMemory());
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to parse CNAB file at line {LineNumber}", lineNumber);
            }

            if (transaction != null)
            {
                yield return transaction;
            }
        }
    }

    /// <summary>
    /// Parses a single fixed-width CNAB line into a Transaction entity.
    /// </summary>
    private static Transaction? ParseCnabLine(ReadOnlyMemory<char> line)
    {
        if (line.Length != CnabLineLength)
        {
            return null;
        }

        /*
        CNAB Format Specification:
        - Pos 1: Type (1 char)                 [Index 0, Length 1]
        - Pos 2-9: Date (8 chars)              [Index 1, Length 8]
        - Pos 10-19: Amount (10 chars)         [Index 9, Length 10]
        - Pos 20-30: CPF (11 chars)            [Index 19, Length 11]
        - Pos 31-42: Card (12 chars)           [Index 30, Length 12]
        - Pos 43-48: Time (6 chars)            [Index 42, Length 6]
        - Pos 49-62: Store Owner (14 chars)    [Index 48, Length 14]
        - Pos 63-81: Store Name (18 chars)     [Index 62, Length 18]
        */
        var transactionTypeSpan = line.Slice(0, 1).Span;
        var dateSpan = line.Slice(1, 8).Span;
        var amountSpan = line.Slice(9, 10).Span;
        var cpf = line.Slice(19, 11).ToString();
        var card = line.Slice(30, 12).ToString();
        var timeSpan = line.Slice(42, 6).Span;
        var owner = line.Slice(48, 14).ToString().TrimEnd();
        var storeName = line.Slice(62, 18).ToString().TrimEnd();

        var transactionType = Enum.Parse<TransactionType>(transactionTypeSpan);

        var year = int.Parse(dateSpan.Slice(0, 4));
        var month = int.Parse(dateSpan.Slice(4, 2));
        var day = int.Parse(dateSpan.Slice(6, 2));
        var hour = int.Parse(timeSpan.Slice(0, 2));
        var minute = int.Parse(timeSpan.Slice(2, 2));
        var second = int.Parse(timeSpan.Slice(4, 2));
        var date = new DateTimeOffset(year, month, day, hour, minute, second, DefaultTimezone);

        var amountValue = decimal.Parse(amountSpan) / 100m;

        var store = new Store
        {
            Name = storeName,
            OwnerName = owner
        };

        var transaction = new Transaction
        {
            Date = date,
            Type = transactionType,
            Amount = amountValue,
            Store = store,
            Cpf = cpf,
            Card = card
        };

        return transaction;
    }
}
