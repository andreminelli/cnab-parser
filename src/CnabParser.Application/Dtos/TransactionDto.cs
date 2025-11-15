namespace CnabParser.Application.Dtos;

public record TransactionDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Cpf { get; set; } = string.Empty;
    public string Card { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
}
