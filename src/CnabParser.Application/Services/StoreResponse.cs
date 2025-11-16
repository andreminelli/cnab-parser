using CnabParser.Core.Entities;
using System.ComponentModel;

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

    [Description("Store name")]
    public string Name { get; set; } = string.Empty;

    [Description("Store owner name")]
    public string OwnerName { get; set; } = string.Empty;

    [Description("Current balance from imported transactions")]
    public decimal Balance { get; set; }

    public TransactionResponse[] Transactions { get; set; } = [];
}
