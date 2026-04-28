using CheltuieliApp.Models;
using CheltuieliApp.Services;

namespace CheltuieliApp.Pages;

public partial class StatementDetailsPage : ContentPage
{
    private readonly ImportService _importService;
    private readonly StatementImportEntity _import;

    public StatementDetailsPage(
        ImportService importService,
        StatementImportEntity import,
        List<TransactionEntity> transactions)
    {
        InitializeComponent();

        _importService = importService;
        _import = import;

        var totalIncome = transactions
            .Where(x => x.Direction == "Credit")
            .Sum(x => x.Amount);

        var totalExpenses = transactions
            .Where(x => x.Direction == "Debit")
            .Sum(x => x.Amount);

        BindingContext = new
        {
            Bank = import.Bank,
            BankColor = GetBankColor(import.Bank),
            AccountIban = import.AccountIban,
            Period = $"{import.PeriodStart:dd.MM.yyyy} - {import.PeriodEnd:dd.MM.yyyy}",
            TransactionCountText = $"{transactions.Count} tranzacții",
            ImportedAtText = $"Importat la {import.ImportedAt:dd.MM.yyyy HH:mm}",
            TotalIncomeText = $"{totalIncome:N2} RON",
            TotalExpensesText = $"{totalExpenses:N2} RON",
            Transactions = transactions
                .OrderByDescending(x => x.TransactionDate)
                .ToList()
        };
    }

    private async void OnBackTapped(object sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        var confirm = await DisplayAlertAsync(
            "Ștergere extras",
            "Sigur vrei să ștergi acest extras și toate tranzacțiile asociate?",
            "Șterge",
            "Anulează");

        if (!confirm)
            return;

        await _importService.DeleteImportAsync(_import.Id);

        await DisplayAlertAsync(
            "Extras șters",
            "Extrasul și tranzacțiile asociate au fost șterse.",
            "OK");

        await Navigation.PopAsync();
    }

    private static string GetBankColor(string bank)
    {
        if (bank.Equals("BT", StringComparison.OrdinalIgnoreCase))
            return "#38BDF8";

        if (bank.Equals("BRD", StringComparison.OrdinalIgnoreCase))
            return "#EF4444";

        if (bank.Contains("Raiffeisen", StringComparison.OrdinalIgnoreCase))
            return "#FACC15";

        return "#9CA3AF";
    }
}