using CheltuieliApp.Models;
using CheltuieliApp.Services;
using System.Text;

namespace CheltuieliApp.Pages;

public partial class StatementsPage : ContentPage
{
    private readonly ImportService _importService;

    public StatementsPage(ImportService importService)
    {
        InitializeComponent();
        _importService = importService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var imports = await _importService.GetImportsAsync();
        ImportsList.ItemsSource = imports;
    }

    private async void OnImportSelected(object sender, SelectionChangedEventArgs e)
    {
        var selectedImport = e.CurrentSelection.FirstOrDefault() as StatementImportEntity;

        if (selectedImport == null)
            return;

        ImportsList.SelectedItem = null;

        var transactions = await _importService
            .GetTransactionsForImportAsync(selectedImport.Id);

        await Navigation.PushAsync(
            new StatementDetailsPage(_importService, selectedImport, transactions));
    }
}