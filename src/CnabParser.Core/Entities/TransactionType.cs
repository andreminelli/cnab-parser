namespace CnabParser.Core.Entities;

public enum TransactionType: byte
{
    Debit = 1,            // Debit (Income)
    Boleto = 2,           // Boleto (Expense)
    Financing = 3,        // Financiamento (Expense)
    Credit = 4,           // Crédito (Income)
    LoanReceipt = 5,      // Recebimento Empréstimo (Income)
    Sales = 6,            // Vendas (Income)
    TedReceipt = 7,       // Recebimento TED (Income)
    DocReceipt = 8,       // Recebimento DOC (Income)
    Rent = 9              // Aluguel (Expense)
}
