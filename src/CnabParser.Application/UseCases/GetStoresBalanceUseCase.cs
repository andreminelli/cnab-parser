using CnabParser.Application.Dtos;
using CnabParser.Core.Repositories;

namespace CnabParser.Application.UseCases;

public interface IGetStoresBalanceUseCase
{
    Task<IEnumerable<StoreBalanceDto>> ExecuteAsync();
}

public class GetStoresBalanceUseCase : IGetStoresBalanceUseCase
{
    private readonly IStoreRepository _storeRepository;

    public GetStoresBalanceUseCase(IStoreRepository storeRepository)
    {
        _storeRepository = storeRepository;
    }

    public async Task<IEnumerable<StoreBalanceDto>> ExecuteAsync()
    {
        var stores = await _storeRepository.GetAllAsync();

        return stores.Select(store => new StoreBalanceDto
        {
            Id = store.Id,
            Name = store.Name,
            Owner = store.Owner,
            Balance = store.GetBalance(),
            Transactions = store.Transactions.Select(t => new TransactionDto
            {
                Id = t.Id,
                Date = t.Date,
                Type = t.Type.ToString(),
                Amount = t.Amount,
                Cpf = t.Cpf,
                Card = t.Card,
                Time = t.Time
            }).ToList()
        }).ToList();
    }
}
