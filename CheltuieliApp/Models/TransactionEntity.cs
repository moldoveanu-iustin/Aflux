using SQLite;
namespace CheltuieliApp.Models;

public class TransactionEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int StatementImportId { get; set; }

    [Indexed]
    public string Bank { get; set; } = "";

    public string AccountIban { get; set; } = "";

    [Indexed]
    public DateTime TransactionDate { get; set; }

    public decimal Amount { get; set; }

    [Indexed]
    public string Direction { get; set; } = "";

    public string Merchant { get; set; } = "";
    public string Description { get; set; } = "";
}