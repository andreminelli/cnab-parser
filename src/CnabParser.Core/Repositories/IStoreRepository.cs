using CnabParser.Core.Entities;

namespace CnabParser.Core.Repositories;

public interface IStoreRepository
{
    Task<IEnumerable<Store>> GetAllAsync();
    Task<Store?> GetByIdAsync(int id);
    Task<Store?> GetByNameAndOwnerAsync(string name, string owner);
    Task AddAsync(Store store);
    Task UpdateAsync(Store store);
}
