namespace CnabParser.Core.Entities;

public class Transaction
{
    public int Id { get; protected set; }

    public required Store Store { get; set; }

    public Guid? DataSourceId { get; set; }

    public DateTimeOffset Date { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string Cpf { get; set; } = string.Empty;
    public string Card { get; set; } = string.Empty;

    internal decimal GetSignedAmount()
    {
        throw new NotImplementedException();
    }
}
