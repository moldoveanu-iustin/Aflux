using SQLite;

namespace CheltuieliApp.Models;

public class StatementImportEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Bank { get; set; } = "";
    public string AccountIban { get; set; } = "";

    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    public int TransactionCount { get; set; }

    public string FileName { get; set; } = "";
    public string FileHash { get; set; } = "";

    public DateTime ImportedAt { get; set; } = DateTime.Now;
}