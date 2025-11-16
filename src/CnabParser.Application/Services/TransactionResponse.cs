using CnabParser.Core.Entities;

namespace CnabParser.Application.Services;

public class TransactionResponse
{
    public TransactionResponse()
    {
    }

    public TransactionResponse(Transaction transaction)
    {
        Type = transaction.Type.ToString();
        Amount = transaction.GetSignedAmount();
        Date = transaction.Date.DateTime;
        Cpf = transaction.Cpf;
        Card = transaction.Card;
    }

    public string Type { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime Date { get; set; }

    public string Cpf { get; set; } = string.Empty;

    public string Card { get; set; } = string.Empty;
}
