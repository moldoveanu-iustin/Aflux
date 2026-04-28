using System.Text;
using CheltuieliApp.DTOs;
using CheltuieliApp.Models;
using CheltuieliApp.Services;

namespace CheltuieliApp.Pages;

public partial class StatementsPage : ContentPage
{
    private readonly ImportService _importService;
    private List<StatementImportEntity> _allImports = new();
    private bool _filtersInitialized;
    private List<FilterOptionDto> _years;
    private List<FilterOptionDto> _months;

    private int? _selectedYear;
    private int? _selectedMonth;

    private bool _isYearFilter;
    public StatementsPage(ImportService importService)
    {
        InitializeComponent();
        _importService = importService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        LoadingStatementsLabel.IsVisible = true;
        ImportsList.IsVisible = false;

        _allImports = await _importService.GetImportsAsync();

        SetupFilters();
        ApplyFilters();

        LoadingStatementsLabel.IsVisible = false;
        ImportsList.IsVisible = true;
    }

    private bool _isOpeningImport;

    private async void OnImportSelected(object sender, SelectionChangedEventArgs e)
    {
        if (_isOpeningImport)
            return;

        var selectedItem = e.CurrentSelection.FirstOrDefault() as StatementImportListItemDto;

        if (selectedItem == null)
            return;

        ImportsList.SelectedItem = null;

        _isOpeningImport = true;
        OpenStatementLoadingOverlay.IsVisible = true;

        try
        {
            await Task.Delay(80);

            var selectedImport = selectedItem.Import;

            var transactions = await _importService
                .GetTransactionsForImportAsync(selectedImport.Id);

            await Navigation.PushAsync(
                new StatementDetailsPage(_importService, selectedImport, transactions));
        }
        finally
        {
            OpenStatementLoadingOverlay.IsVisible = false;
            _isOpeningImport = false;
        }
    }

    private void SetupFilters()
    {
        _selectedYear = null;
        _selectedMonth = null;

        SelectedYearLabel.Text = "Toți anii";
        SelectedMonthLabel.Text = "Toate lunile";

        _filtersInitialized = false;

        var years = _allImports
            .SelectMany(x => new[] { x.PeriodStart.Year, x.PeriodEnd.Year })
            .Distinct()
            .OrderByDescending(x => x)
            .Select(x => new FilterOptionDto { Name = x.ToString(), Value = x })
            .ToList();

        years.Insert(0, new FilterOptionDto { Name = "Toți anii", Value = null });


        var months = new List<FilterOptionDto>
    {
        new() { Name = "Toate lunile", Value = null },
        new() { Name = "Ianuarie", Value = 1 },
        new() { Name = "Februarie", Value = 2 },
        new() { Name = "Martie", Value = 3 },
        new() { Name = "Aprilie", Value = 4 },
        new() { Name = "Mai", Value = 5 },
        new() { Name = "Iunie", Value = 6 },
        new() { Name = "Iulie", Value = 7 },
        new() { Name = "August", Value = 8 },
        new() { Name = "Septembrie", Value = 9 },
        new() { Name = "Octombrie", Value = 10 },
        new() { Name = "Noiembrie", Value = 11 },
        new() { Name = "Decembrie", Value = 12 }
    };

        _years = years;
        _months = months;

        _filtersInitialized = true;
    }

    private void OnFiltersChanged(object sender, EventArgs e)
    {
        if (!_filtersInitialized)
            return;

        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var selectedYear = _selectedYear;
        var selectedMonth = _selectedMonth;

        var filtered = _allImports.AsEnumerable();

        if (selectedYear.HasValue)
        {
            filtered = filtered.Where(x =>
                x.PeriodStart.Year == selectedYear.Value ||
                x.PeriodEnd.Year == selectedYear.Value);
        }

        if (selectedMonth.HasValue)
        {
            filtered = filtered.Where(x =>
                DateRangeContainsMonth(x.PeriodStart, x.PeriodEnd, selectedMonth.Value, selectedYear));
        }

        var filteredList = filtered
            .OrderByDescending(x => x.ImportedAt)
            .ToList();

        FilteredImportsCountLabel.Text = filteredList.Count.ToString();
        FilteredTransactionsCountLabel.Text = filteredList.Sum(x => x.TransactionCount).ToString();

        ImportsList.ItemsSource = filteredList
            .Select(x => new StatementImportListItemDto { Import = x })
            .ToList();
    }

    private static bool DateRangeContainsMonth(DateTime start, DateTime end, int month, int? year)
    {
        var cursor = new DateTime(start.Year, start.Month, 1);
        var last = new DateTime(end.Year, end.Month, 1);

        while (cursor <= last)
        {
            if (cursor.Month == month && (!year.HasValue || cursor.Year == year.Value))
                return true;

            cursor = cursor.AddMonths(1);
        }

        return false;
    }

    // handlere filtre
    private async void OnYearTapped(object sender, EventArgs e)
    {
        _isYearFilter = true;
        FilterTitleLabel.Text = "Selectează anul";

        FilterList.ItemsSource = _years;

        await OpenFilterSheet();
    }

    private async void OnMonthTapped(object sender, EventArgs e)
    {
        _isYearFilter = false;
        FilterTitleLabel.Text = "Selectează luna";

        FilterList.ItemsSource = _months;

        await OpenFilterSheet();
    }
    private async void OnFilterSelected(object sender, EventArgs e)
    {
        if (sender is not Border border)
            return;

        if (border.BindingContext is not FilterOptionDto option)
            return;

        if (_isYearFilter)
        {
            SelectedYearLabel.Text = option.Name;
            _selectedYear = option.Value as int?;
        }
        else
        {
            SelectedMonthLabel.Text = option.Name;
            _selectedMonth = option.Value as int?;
        }

        await CloseFilterSheet();

        ApplyFilters();
    }
    private async Task OpenFilterSheet()
    {
        FilterOverlay.IsVisible = true;

        FilterBottomSheet.TranslationY = 400;

        await Task.WhenAll(
            FilterOverlay.FadeTo(1, 120),
            FilterBottomSheet.TranslateTo(0, 0, 200, Easing.CubicOut)
        );
    }

    private async Task CloseFilterSheet()
    {
        await Task.WhenAll(
            FilterOverlay.FadeTo(0, 120),
            FilterBottomSheet.TranslateTo(0, 400, 160, Easing.CubicIn)
        );

        FilterOverlay.IsVisible = false;
    }

    private async void OnCloseFilterClicked(object sender, EventArgs e)
    {
        await CloseFilterSheet();
    }
}