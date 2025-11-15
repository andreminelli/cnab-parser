using CnabParser.Core.Entities;

namespace CnabParser.Application;

public interface ICnabFileParser
{
    IAsyncEnumerable<Transaction> ParseAsync(Stream fileStream);
}
