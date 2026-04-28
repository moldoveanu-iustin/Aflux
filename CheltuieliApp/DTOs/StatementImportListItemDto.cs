using CheltuieliApp.Models;

namespace CheltuieliApp.DTOs;

public class StatementImportListItemDto
{
    public StatementImportEntity Import { get; set; } = new();

    public int Id => Import.Id;
    public string Bank => Import.Bank;
    public string AccountIban => Import.AccountIban;
    public int TransactionCount => Import.TransactionCount;

    public string BankColor => Bank.ToUpperInvariant() switch
    {
        "BT" => "#38BDF8",
        "BRD" => "#EF4444",
        _ when Bank.Contains("Raiffeisen", StringComparison.OrdinalIgnoreCase) => "#FACC15",
        _ => "#9CA3AF"
    };

    public string PeriodText =>
        $"{Import.PeriodStart:dd.MM.yyyy} - {Import.PeriodEnd:dd.MM.yyyy}";

    public string TransactionCountText =>
        $"{Import.TransactionCount} tranzacții";

    public string ImportedAtText =>
        $"Importat: {Import.ImportedAt:dd.MM.yyyy}";
}