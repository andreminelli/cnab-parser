using CnabParser.Core.Entities;

namespace CnabParser.Core.Repositories;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction);
}
