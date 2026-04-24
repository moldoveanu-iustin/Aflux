using CheltuieliApp.DTOs;
using CheltuieliApp.Helpers;
using CheltuieliApp.Parsers;
using CheltuieliApp.Services;

namespace CheltuieliApp.Pages;

public partial class ImportPage : ContentPage
{
    private BankStatementDto? _currentStatement;
    private string _currentFileName = "";
    private string _currentFileHash = "";
    private readonly ImportService _importService;

    public ImportPage(ImportService importService)
    {
        InitializeComponent();
        _importService = importService;
    }

    private async void OnImportPdfClicked(object sender, EventArgs e)
    {
        try
        {
            var file = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Alege extrasul PDF",
                FileTypes = FilePickerFileType.Pdf
            });

            if (file == null)
                return;

            await using var stream = await file.OpenReadAsync();

            var text = PdfTextExtractor.ExtractText(stream);

            var factory = new BankStatementParserFactory();
            var statement = factory.Parse(text);

            _currentStatement = statement;
            _currentFileName = file.FileName;
            _currentFileHash = HashHelper.GenerateHash(text);
            ConfirmImportButton.IsVisible = statement.Transactions.Any();

            ResultLabel.Text =
                $"Bancă: {statement.Bank}\n" +
                $"IBAN: {statement.AccountIban}\n" +
                $"Perioadă: {statement.PeriodStart:dd.MM.yyyy} - {statement.PeriodEnd:dd.MM.yyyy}\n" +
                $"Tranzacții: {statement.Transactions.Count}";

            TransactionsList.ItemsSource = statement.Transactions;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Eroare", ex.Message, "OK");
        }
    }
    private async void OnConfirmImportClicked(object sender, EventArgs e)
    {
        if (_currentStatement == null)
            return;

        await _importService.SaveStatementAsync(
            _currentStatement,
            _currentFileName,
            _currentFileHash);

        await DisplayAlertAsync("Succes", "Extrasul a fost salvat.", "OK");

        ConfirmImportButton.IsVisible = false;
    }
}