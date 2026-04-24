using CheltuieliApp.Helpers;
using CheltuieliApp.Parsers;

namespace CheltuieliApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
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

            System.Diagnostics.Debug.WriteLine(text);

            var factory = new BankStatementParserFactory();
            var statement = factory.Parse(text);

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
}