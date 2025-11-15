namespace CnabParser.Core.Entities;

/// <summary>
/// Represents a CNAB transaction with embedded store information.
/// </summary>
public class Transaction
{
    public int Id { get; set; }
    
    // Store Information
    public string StoreOwner { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;

    // Transaction Details
    public DateTime Date { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string Cpf { get; set; } = string.Empty;
    public string Card { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;

    /// <summary>
    /// Gets the signed amount based on transaction type.
    /// </summary>
    public decimal GetSignedAmount()
    {
        return Type switch
        {
            TransactionType.Debit => Amount,              // + (Entrada)
            TransactionType.Boleto => -Amount,            // - (Saída)
            TransactionType.Financiamento => -Amount,     // - (Saída)
            TransactionType.Credit => Amount,             // + (Entrada)
            TransactionType.LoanReceipt => Amount,         // + (Entrada)
            TransactionType.Sales => Amount,              // + (Entrada)
            TransactionType.TedReceipt => Amount,          // + (Entrada)
            TransactionType.DocReceipt => Amount,          // + (Entrada)
            TransactionType.Rent => -Amount,              // - (Saída)
            _ => 0
        };
    }
}

/// <summary>
/// CNAB Transaction Types based on official specification
/// </summary>
public enum TransactionType
{
    Debit = 1,           // Débito (Entrada) +
    Boleto = 2,           // Boleto (Saída) -
    Financiamento = 3,    // Financiamento (Saída) -
    Credit = 4,           // Crédito (Entrada) +
    LoanReceipt = 5,      // Recebimento Empréstimo (Entrada) +
    Sales = 6,            // Vendas (Entrada) +
    TedReceipt = 7,       // Recebimento TED (Entrada) +
    DocReceipt = 8,       // Recebimento DOC (Entrada) +
    Rent = 9              // Aluguel (Saída) -
}
