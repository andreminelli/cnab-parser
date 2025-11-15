using CnabParser.Core.Entities;
using CnabParser.Core.Repositories;
using CnabParser.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CnabParser.Infrastructure.Repositories;

public class StoreRepository : IStoreRepository
{
    private readonly CnabParserDbContext _context;

    public StoreRepository(CnabParserDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Store>> GetAllAsync()
    {
        return await _context.Stores
            .Include(s => s.Transactions)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Store?> GetByIdAsync(int id)
    {
        return await _context.Stores
            .Include(s => s.Transactions)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Store?> GetByNameAndOwnerAsync(string name, string owner)
    {
        return await _context.Stores
            .Include(s => s.Transactions)
            .FirstOrDefaultAsync(s => s.Name == name && s.Owner == owner);
    }

    public async Task AddAsync(Store store)
    {
        await _context.Stores.AddAsync(store);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Store store)
    {
        _context.Stores.Update(store);
        await _context.SaveChangesAsync();
    }
}
