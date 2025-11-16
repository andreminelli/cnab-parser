namespace CnabParser.Core.Entities;

public static class TransactionExtensions
{
    public static decimal GetBalance(this IEnumerable<Transaction> transactions)
        => transactions
            .Aggregate(
                0m,
                (sum, transaction) => sum + transaction.GetSignedAmount());
}
