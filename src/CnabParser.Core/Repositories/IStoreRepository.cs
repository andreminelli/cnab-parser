using CnabParser.Core.Entities;

namespace CnabParser.Core.Repositories;

public interface IStoreRepository
{
    Task<Store> GetOrCreateAsync(Store store);
}
