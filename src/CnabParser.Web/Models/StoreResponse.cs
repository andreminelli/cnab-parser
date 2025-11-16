namespace CnabParser.Web.Models;

public class StoreResponse
{
    public required string Name { get; set; }
    public required string OwnerName { get; set; }
    public decimal Balance { get; set; }
    public List<TransactionResponse> Transactions { get; set; } = new();
}
