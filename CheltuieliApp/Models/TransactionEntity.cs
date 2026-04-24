using SQLite;

namespace CheltuieliApp.Models;

public class TransactionEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int StatementImportId { get; set; }

    public string Bank { get; set; } = "";
    public string AccountIban { get; set; } = "";

    public DateTime TransactionDate { get; set; }
    public decimal Amount { get; set; }

    public string Direction { get; set; } = "";
    public string Merchant { get; set; } = "";
    public string Description { get; set; } = "";
}