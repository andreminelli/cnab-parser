using System.Globalization;
using CnabParser.Core.Entities;
using CnabParser.Core.Services;

namespace CnabParser.Infrastructure.Services;

/// <summary>
/// Service for parsing CNAB files using fixed-width format.
/// Parses fixed-width records directly without external libraries for format mapping.
/// </summary>
public class CnabParserService : ICnabParserService
{
    /// <summary>
    /// Parses a CNAB file stream and returns all transactions with embedded store information.
    /// </summary>
    public async Task<IEnumerable<Transaction>> ParseCnabFileAsync(Stream fileStream)
    {
        var transactions = new List<Transaction>();

        using var reader = new StreamReader(fileStream);
        string? line;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            // Skip empty lines
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var transaction = ParseCnabLine(line);
            if (transaction != null)
            {
                transactions.Add(transaction);
            }
        }

        return transactions;
    }

    /// <summary>
    /// Parses a single fixed-width CNAB line into a Transaction entity.
    /// 
    /// CNAB Format Specification (1-indexed as per spec):
    /// - Pos 1: Type (1 char)                 [Index 0, Length 1]
    /// - Pos 2-9: Date (8 chars)              [Index 1, Length 8]
    /// - Pos 10-19: Amount (10 chars)         [Index 9, Length 10]
    /// - Pos 20-30: CPF (11 chars)            [Index 19, Length 11]
    /// - Pos 31-42: Card (12 chars)           [Index 30, Length 12]
    /// - Pos 43-48: Time (6 chars)            [Index 42, Length 6]
    /// - Pos 49-62: Store Owner (14 chars)    [Index 48, Length 14]
    /// - Pos 63-81: Store Name (19 chars)     [Index 62, Length 19]
    /// </summary>
    private Transaction? ParseCnabLine(string line)
    {
        try
        {
            // Ensure line is long enough
            if (line.Length < 81)
                return null;

            // Extract fixed-width fields (converting 1-indexed to 0-indexed)
            var typeChar = line[0];
            var dateStr = line.Substring(1, 8);
            var amountStr = line.Substring(9, 10);
            var cpfStr = line.Substring(19, 11);
            var cardStr = line.Substring(30, 12);
            var timeStr = line.Substring(42, 6);
            var ownerStr = line.Substring(48, 14);
            var storeNameStr = line.Length > 62 ? line.Substring(62, Math.Min(19, line.Length - 62)) : "";

            // Parse transaction type
            var typeCode = int.Parse(typeChar.ToString());
            var transactionType = GetTransactionType(typeCode);

            // Parse date (YYYYMMDD format)
            var year = int.Parse(dateStr.Substring(0, 4));
            var month = int.Parse(dateStr.Substring(4, 2));
            var day = int.Parse(dateStr.Substring(6, 2));
            var date = new DateTime(year, month, day);

            // Parse amount: divide by 100 to normalize
            var amountValue = string.IsNullOrWhiteSpace(amountStr) 
                ? 0m 
                : decimal.Parse(amountStr) / 100m;

            var transaction = new Transaction
            {
                Date = date,
                Type = transactionType,
                Amount = amountValue,
                StoreName = storeNameStr.Trim(),
                StoreOwner = ownerStr.Trim(),
                Cpf = cpfStr.Trim(),
                Card = cardStr.Trim(),
                Time = timeStr.Trim()
            };

            return transaction;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error parsing CNAB line: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Maps numeric transaction type code to TransactionType enum.
    /// Based on CNAB official specification.
    /// </summary>
    private static TransactionType GetTransactionType(int code)
    {
        return code switch
        {
            1 => TransactionType.Debit,           // Débito (Entrada) +
            2 => TransactionType.Boleto,          // Boleto (Saída) -
            3 => TransactionType.Financiamento,   // Financiamento (Saída) -
            4 => TransactionType.Credit,          // Crédito (Entrada) +
            5 => TransactionType.LoanReceipt,     // Recebimento Empréstimo (Entrada) +
            6 => TransactionType.Sales,           // Vendas (Entrada) +
            7 => TransactionType.TedReceipt,      // Recebimento TED (Entrada) +
            8 => TransactionType.DocReceipt,      // Recebimento DOC (Entrada) +
            9 => TransactionType.Rent,            // Aluguel (Saída) -
            _ => TransactionType.Credit
        };
    }
}
