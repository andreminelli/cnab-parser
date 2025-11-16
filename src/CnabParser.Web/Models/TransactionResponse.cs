namespace CnabParser.Web.Models;

public class TransactionResponse
{
    public required string Type { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public required string Cpf { get; set; }
    public required string Card { get; set; }
}
