using SQLite;
namespace CheltuieliApp.Models;

public class StatementImportEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public string Bank { get; set; } = "";

    [Indexed]
    public string AccountIban { get; set; } = "";

    [Indexed]
    public DateTime PeriodStart { get; set; }

    [Indexed]
    public DateTime PeriodEnd { get; set; }

    public int TransactionCount { get; set; }

    public string FileName { get; set; } = "";

    [Indexed]
    public string FileHash { get; set; } = "";

    public DateTime ImportedAt { get; set; } = DateTime.Now;
}