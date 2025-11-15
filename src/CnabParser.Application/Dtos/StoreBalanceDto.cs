namespace CnabParser.Application.Dtos;

public record StoreBalanceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public IEnumerable<TransactionDto> Transactions { get; set; } = new List<TransactionDto>();
}
