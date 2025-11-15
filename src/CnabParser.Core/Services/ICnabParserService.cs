using CnabParser.Core.Entities;

namespace CnabParser.Core.Services;

/// <summary>
/// Service for parsing CNAB files.
/// </summary>
public interface ICnabParserService
{
    /// <summary>
    /// Parses a CNAB file stream and returns all transactions.
    /// Each transaction contains embedded store information.
    /// </summary>
    Task<IEnumerable<Transaction>> ParseCnabFileAsync(Stream fileStream);
}
