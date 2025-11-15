namespace CnabParser.Core.Entities;

/// <summary>
/// Represents a store that performs financial transactions.
/// </summary>
public class Store
{
    public int Id { get; set; }
    public string Owner { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// All transactions associated with this store.
    /// </summary>
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    /// <summary>
    /// Calculates the total balance for this store based on all transactions.
    /// </summary>
    public decimal GetBalance()
    {
        return Transactions.Sum(t => t.GetSignedAmount());
    }
}
