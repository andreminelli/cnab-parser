using CnabParser.Core.Entities;
using CnabParser.Core.Repositories;

namespace CnabParser.Infrastructure.Repositories;

public class TransactionRepository(CnabDbContext context) : ITransactionRepository
{
    private readonly CnabDbContext _context = context;

    public async Task AddAsync(Transaction transaction)
    {
        await _context.Transactions.AddAsync(transaction);
    }
}
