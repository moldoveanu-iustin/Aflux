using CheltuieliApp.DTOs;
using CheltuieliApp.Helpers;
using CheltuieliApp.Models;
using CheltuieliApp.Parsers;
using CheltuieliApp.Services;

namespace CheltuieliApp.Pages;

public partial class ImportPage : ContentPage
{
    private BankStatementDto? _currentStatement;
    private string _currentFileName = "";
    private string _currentFileHash = "";
    private readonly ImportService _importService;
    private readonly CategoryService _categoryService;

    private BankTransactionDto? _selectedTransactionForCategory;
    private List<CategoryEntity> _categories = new();

    public ImportPage(ImportService importService, CategoryService categoryService)
    {
        InitializeComponent();
        _importService = importService;
        _categoryService = categoryService;
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

            ResultLabel.Text = "Se încarcă extrasul...";
            ConfirmImportButton.IsVisible = false;
            TransactionsList.ItemsSource = null;

            await using var stream = await file.OpenReadAsync();

            var text = PdfTextExtractor.ExtractText(stream);

            var factory = new BankStatementParserFactory();
            var statement = factory.Parse(text);

            await _categoryService.ApplyCategoriesAsync(statement.Transactions);

            _currentStatement = statement;
            _currentFileName = file.FileName;
            _currentFileHash = HashHelper.GenerateHash(text);
            ConfirmImportButton.IsVisible = statement.Transactions.Any();

            //validare perioada si banca extras de cont
            var validation = await _importService.ValidateImportAsync(_currentStatement);

            if (validation.Status == ImportValidationStatus.FullyCovered)
            {
                await DisplayAlertAsync("Import blocat", validation.Message, "OK");
                return;
            }
            //validare

            ResultLabel.Text =
                $"Bancă: {statement.Bank}\n" +
                $"IBAN: {statement.AccountIban}\n" +
                $"Perioadă: {statement.PeriodStart:dd.MM.yyyy} - {statement.PeriodEnd:dd.MM.yyyy}\n" +
                $"Tranzacții: {statement.Transactions.Count}";

            TransactionsList.ItemsSource = statement.Transactions;
            TransactionsList.IsVisible = true;
            ConfirmImportButton.IsVisible = true;
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

        var validation = await _importService.ValidateImportAsync(_currentStatement);

        if (validation.Status == ImportValidationStatus.FullyCovered)
        {
            await DisplayAlertAsync("Import blocat", validation.Message, "OK");
            return;
        }

        DateTime? allowedStart = null;
        DateTime? allowedEnd = null;

        if (validation.Status == ImportValidationStatus.PartiallyCovered)
        {
            var confirm = await DisplayAlertAsync(
                "Suprapunere detectată",
                validation.Message,
                "Importă doar zilele noi",
                "Anulează");

            if (!confirm)
                return;

            allowedStart = validation.AllowedStart;
            allowedEnd = validation.AllowedEnd;
        }

        if (_currentStatement.Transactions.Any(x => x.CategoryId == null))
        {
            await DisplayAlertAsync("Categorii lipsă", "Există tranzacții fără categorie. Completează toate categoriile înainte de salvare.", "OK");

            return;
        }

        await _importService.SaveStatementAsync(_currentStatement, _currentFileName, _currentFileHash, allowedStart, allowedEnd);

        await DisplayAlertAsync("Succes", "Extrasul a fost salvat.", "OK");

        ConfirmImportButton.IsVisible = false;
    }
    private void ApplyCategoryToMatchingPreviewTransactions(string keyword, int categoryId, string categoryName)
    {
        if (_currentStatement == null)
            return;

        keyword = keyword.ToUpperInvariant();

        foreach (var transaction in _currentStatement.Transactions)
        {
            var searchableText =
                $"{transaction.Merchant} {transaction.Description} {transaction.RawText}"
                .ToUpperInvariant();

            if (searchableText.Contains(keyword))
            {
                transaction.CategoryId = categoryId;
                transaction.CategoryName = categoryName;
            }
        }
    }
    private async void OnChangeCategoryClicked(object sender, EventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.BindingContext is not BankTransactionDto transaction)
            return;

        _selectedTransactionForCategory = transaction;

        _categories = await _categoryService.GetCategoriesAsync();
        CategoriesList.ItemsSource = _categories;
        CategoriesList.SelectedItem = null;

        SelectedTransactionMerchantLabel.Text = transaction.Merchant;
        SelectedTransactionAmountLabel.Text =
            $"{(transaction.Direction == "Credit" ? "+" : "-")}{transaction.Amount:N2} RON";
        SelectedTransactionAmountLabel.TextColor =
            transaction.Direction == "Credit" ? Colors.Green : Colors.Red;

        SelectedTransactionDescriptionLabel.Text = transaction.Description;

        var keyword = MerchantKeywordHelper.SuggestKeyword(transaction.Merchant);
        SuggestedKeywordLabel.Text = string.IsNullOrWhiteSpace(keyword)
            ? "Nu am putut sugera automat un keyword."
            : $"Keyword regulă: {keyword}";

        SaveRuleCheckBox.IsToggled = true;

        await OpenCategorySheetAsync();
    }
    private void RefreshPreviewTransactions()
    {
        if (_currentStatement == null)
            return;

        TransactionsList.ItemsSource = null;
        TransactionsList.ItemsSource = _currentStatement.Transactions;
    }
    private async Task OpenCategorySheetAsync()
    {
        CategoryOverlay.IsVisible = true;

        CategoryBottomSheet.TranslationY = 520;
        CategoryOverlay.Opacity = 0;

        await Task.WhenAll(
            CategoryOverlay.FadeTo(1, 150),
            CategoryBottomSheet.TranslateTo(0, 0, 220, Easing.CubicOut)
        );
    }
    private async Task CloseCategorySheetAsync()
    {
        await Task.WhenAll(
            CategoryOverlay.FadeTo(0, 120),
            CategoryBottomSheet.TranslateTo(0, 520, 180, Easing.CubicIn)
        );

        CategoryOverlay.IsVisible = false;
    }
    private async void OnCloseCategorySheetClicked(object sender, EventArgs e)
    {
        await CloseCategorySheetAsync();
    }

    private async void OnCategoryTapped(object sender, TappedEventArgs e)
    {
        if (sender is not Border border)
            return;

        if (border.BindingContext is not CategoryEntity category)
            return;

        border.BackgroundColor = Color.FromArgb("#EEF2FF");
        border.Stroke = Color.FromArgb("#3F51B5");
        border.StrokeThickness = 1;

        await border.ScaleTo(0.97, 70);
        await border.ScaleTo(1, 90);

        await ApplySelectedCategoryAsync(category);
    }
    private async Task ApplySelectedCategoryAsync(CategoryEntity category)
    {
        if (_selectedTransactionForCategory == null)
            return;

        var keyword = MerchantKeywordHelper.SuggestKeyword(_selectedTransactionForCategory.Merchant);

        if (SaveRuleCheckBox.IsToggled && !string.IsNullOrWhiteSpace(keyword))
        {
            await _categoryService.AddMerchantRuleAsync(keyword, category.Id, _selectedTransactionForCategory.Bank);

            ApplyCategoryToMatchingPreviewTransactions(
                keyword,
                category.Id,
                category.Name);
        }
        else
        {
            _selectedTransactionForCategory.CategoryId = category.Id;
            _selectedTransactionForCategory.CategoryName = category.Name;
        }

        RefreshPreviewTransactions();

        await CloseCategorySheetAsync();
    }
}