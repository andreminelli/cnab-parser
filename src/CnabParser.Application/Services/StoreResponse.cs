using CnabParser.Core.Entities;

namespace CnabParser.Application.Services;

public class StoreResponse
{
    public StoreResponse(Store store, IEnumerable<Transaction> transactions)
    {
        Name = store.Name;
        OwnerName = store.OwnerName;
        Balance = transactions.GetBalance();
        Transactions = transactions
            .Select(t => new TransactionResponse(t))
            .ToArray();
    }

    public string Name { get; }

    public string OwnerName { get; }

    public decimal Balance { get; }

    public TransactionResponse[] Transactions { get; }
}
