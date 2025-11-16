using CnabParser.Core.Entities;

namespace CnabParser.Application.Helpers;

public interface ICnabFileParser
{
    IAsyncEnumerable<Transaction> ParseAsync(Stream fileStream);
}
