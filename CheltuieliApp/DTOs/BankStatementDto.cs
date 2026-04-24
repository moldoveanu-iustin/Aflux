// DTOs/BankStatementDto.cs
namespace CheltuieliApp.DTOs;

public class BankStatementDto
{
    public string Bank { get; set; } = "";
    public string AccountIban { get; set; } = "";

    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    public List<BankTransactionDto> Transactions { get; set; } = new();
}