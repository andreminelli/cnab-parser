using CnabParser.Core.Entities;

namespace CnabParser.Core;

public interface ICnabFileParser
{
    IAsyncEnumerable<Transaction> ParseAsync(Stream fileStream);
}
