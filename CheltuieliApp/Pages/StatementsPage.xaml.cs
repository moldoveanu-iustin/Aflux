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
    public StatementsPage(ImportService importService)
    {
        InitializeComponent();
        _importService = importService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _allImports = await _importService.GetImportsAsync();

        SetupFilters();
        ApplyFilters();
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

    private void SetupFilters()
    {
        _filtersInitialized = false;

        var years = _allImports
            .SelectMany(x => new[] { x.PeriodStart.Year, x.PeriodEnd.Year })
            .Distinct()
            .OrderByDescending(x => x)
            .Select(x => new FilterOptionDto { Name = x.ToString(), Value = x })
            .ToList();

        years.Insert(0, new FilterOptionDto { Name = "Toți anii", Value = null });

        YearPicker.ItemsSource = years;
        YearPicker.SelectedIndex = 0;

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

        MonthPicker.ItemsSource = months;
        MonthPicker.SelectedIndex = 0;

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
        var selectedYear = (YearPicker.SelectedItem as FilterOptionDto)?.Value;
        var selectedMonth = (MonthPicker.SelectedItem as FilterOptionDto)?.Value;

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

        ImportsList.ItemsSource = filtered
            .OrderByDescending(x => x.ImportedAt)
            .ToList();
    }

    private static bool DateRangeContainsMonth(
    DateTime start,
    DateTime end,
    int month,
    int? year)
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
}