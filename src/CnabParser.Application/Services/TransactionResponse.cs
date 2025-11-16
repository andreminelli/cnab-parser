using CnabParser.Core.Entities;

namespace CnabParser.Application.Services;

public class TransactionResponse(Transaction transaction)
{
    public string Type => transaction.Type.ToString();

    public decimal Amount => transaction.GetSignedAmount();

    public DateTime Date => transaction.Date.DateTime;

    public string Cpf => transaction.Cpf;

    public string Card => transaction.Card;
}
