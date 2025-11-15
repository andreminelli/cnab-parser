using CnabParser.Core.Repositories;
using CnabParser.Infrastructure.Repositories;

namespace CnabParser.Infrastructure.Repositories;

public class UnitOfWork(CnabDbContext context) : IUnitOfWork
{
    private readonly CnabDbContext _context = context;

    public async Task BeginTransactionAsync()
        => await _context.Database.BeginTransactionAsync();

    public async Task CommitAsync()
    {
        await _context.SaveChangesAsync();
        await _context.Database.CommitTransactionAsync();
    }

    public async Task RollbackAsync()
        => await _context.Database.RollbackTransactionAsync();
}
