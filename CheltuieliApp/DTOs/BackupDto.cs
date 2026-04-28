using CheltuieliApp.Models;

namespace CheltuieliApp.DTOs;

public class BackupDto
{
    public DateTime ExportedAt { get; set; } = DateTime.Now;
    public string AppName { get; set; } = "Aflux";
    public string Version { get; set; } = "1.0.0";

    public List<StatementImportEntity> StatementImports { get; set; } = new();
    public List<TransactionEntity> Transactions { get; set; } = new();
    public List<CategoryEntity> Categories { get; set; } = new();
    public List<MerchantRuleEntity> MerchantRules { get; set; } = new();
}