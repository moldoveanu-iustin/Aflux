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

        BindingContext = new
        {
            Bank = import.Bank,
            Period = $"{import.PeriodStart:dd.MM.yyyy} - {import.PeriodEnd:dd.MM.yyyy}",
            Transactions = transactions
        };
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        var confirm = await DisplayAlert(
            "Confirmare",
            "Ștergi extrasul?",
            "Da",
            "Nu");

        if (!confirm)
            return;

        await _importService.DeleteImportAsync(_import.Id);

        await Navigation.PopAsync();
    }
}