using CnabParser.Core.Entities;
using CnabParser.Core.Repositories;
using CnabParser.Infrastructure.Data;

namespace CnabParser.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly CnabParserDbContext _context;

    public TransactionRepository(CnabParserDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Transaction transaction)
    {
        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<Transaction> transactions)
    {
        await _context.Transactions.AddRangeAsync(transactions);
        await _context.SaveChangesAsync();
    }
}
