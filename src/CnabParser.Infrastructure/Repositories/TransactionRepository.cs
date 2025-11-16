using CnabParser.Core.Entities;
using CnabParser.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CnabParser.Infrastructure.Repositories;

public class TransactionRepository(CnabDbContext context) : ITransactionRepository
{
    private readonly CnabDbContext _context = context;

    public async Task AddAsync(Transaction transaction)
    {
        await _context.Transactions.AddAsync(transaction);
    }

    public IAsyncEnumerable<Transaction> GetByDataSourceIdAsync(Guid importId, CancellationToken cancellationToken)
        => _context.Transactions
            .Include(t => t.Store)
            .Where(t => t.DataSourceId == importId)
            .OrderBy(t => t.Store.Name)
            .ThenBy(t => t.Date)
            .AsAsyncEnumerable();
}
