using CnabParser.Core.Entities;
using CnabParser.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CnabParser.Infrastructure.Repositories;

public class StoreRepository(CnabDbContext context) : IStoreRepository
{
    private readonly CnabDbContext _context = context;
    private readonly Dictionary<(string Name, string OwnerName), Store> _cache = [];

    public async Task<Store> GetOrCreateAsync(Store store)
    {
        var storeKey = (store.Name, store.OwnerName);

        // Check in-memory cache first
        if (_cache.TryGetValue(storeKey, out var cachedStore))
        {
            return cachedStore;
        }

        // Check if Store exists in database
        var existingStore = await _context.Stores
            .FirstOrDefaultAsync(s => s.Name == store.Name && s.OwnerName == store.OwnerName);

        if (existingStore != null)
        {
            _cache[storeKey] = existingStore;
            return existingStore;
        }

        // Add new Store
        _context.Stores.Add(store);
        _cache[storeKey] = store;
        return store;
    }
}
