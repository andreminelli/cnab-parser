using CnabParser.Core.Entities;

namespace CnabParser.Application.Services;

public class StoreResponse
{
    public StoreResponse()
    {
    }

    public StoreResponse(Store store, IEnumerable<Transaction> transactions)
    {
        Name = store.Name;
        OwnerName = store.OwnerName;
        Balance = transactions.GetBalance();
        Transactions = transactions
            .Select(t => new TransactionResponse(t))
            .ToArray();
    }

    public string Name { get; set; } = string.Empty;

    public string OwnerName { get; set; } = string.Empty;

    public decimal Balance { get; set; }

    public TransactionResponse[] Transactions { get; set; } = [];
}
