using CnabParser.Core.Entities;
using System.ComponentModel;

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

    [Description("Type of the transaction")]
    public string Type { get; set; } = string.Empty;

    [Description("Absolute value for the transaction")]
    public decimal Amount { get; set; }

    [Description("Date/time when transaction occured (GMT -3)")]
    public DateTime Date { get; set; }

    [Description("CPF of the beneficiary of the transaction")]
    public string Cpf { get; set; } = string.Empty;

    [Description("Card number (redacted) used in the transaction")]
    public string Card { get; set; } = string.Empty;
}
