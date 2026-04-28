using CheltuieliApp.DTOs;
using CheltuieliApp.Models;
using CheltuieliApp.Services;

namespace CheltuieliApp.Pages;

public partial class TransactionsPage : ContentPage
{
    private readonly ImportService _importService;
    private readonly CategoryService _categoryService;

    private List<TransactionEntity> _allTransactions = new();
    private List<FilterOptionDto> _years = new();
    private List<FilterOptionDto> _months = new();
    private List<FilterOptionDto> _types = new();
    private List<FilterOptionDto> _categories = new();
    private List<SortOptionDto> _sorts = new();

    private string _activeFilter = "";

    private int? _selectedYear;
    private int? _selectedMonth;
    private string? _selectedDirection;
    private int? _selectedCategoryId;
    private string _selectedSort = "newest";

    public TransactionsPage(ImportService importService, CategoryService categoryService)
    {
        InitializeComponent();

        _importService = importService;
        _categoryService = categoryService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        LoadingTransactionsLabel.IsVisible = true;
        TransactionsList.IsVisible = false;

        _allTransactions = await _importService.GetAllTransactionsAsync();

        await SetupFiltersAsync();
        await ApplyFiltersWithLoadingAsync();

        LoadingTransactionsLabel.IsVisible = false;
        TransactionsList.IsVisible = true;
    }

    private async Task SetupFiltersAsync()
    {
        _years = _allTransactions
            .Select(x => x.TransactionDate.Year)
            .Distinct()
            .OrderByDescending(x => x)
            .Select(x => new FilterOptionDto { Name = x.ToString(), Value = x })
            .ToList();

        _years.Insert(0, new FilterOptionDto { Name = "Toți anii", Value = null });

        _months =
        [
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
        ];

        _types =
        [
            new() { Name = "Toate tipurile", Value = null },
            new() { Name = "Venituri", Value = 1 },
            new() { Name = "Cheltuieli", Value = 2 }
        ];

        var categories = await _categoryService.GetCategoriesAsync();

        _categories = categories
            .Select(x => new FilterOptionDto { Name = x.Name, Value = x.Id })
            .ToList();

        _categories.Insert(0, new FilterOptionDto { Name = "Toate categoriile", Value = null });

        _sorts =
        [
            new() { Name = "Cele mai noi", Value = "newest" },
            new() { Name = "Cele mai vechi", Value = "oldest" },
            new() { Name = "Sumă descrescător", Value = "amount_desc" },
            new() { Name = "Sumă crescător", Value = "amount_asc" }
        ];
    }

    private void ApplyFilters()
    {
        var filtered = _allTransactions.AsEnumerable();

        var search = SearchEntry.Text?.Trim();

        if (!string.IsNullOrWhiteSpace(search))
        {
            filtered = filtered.Where(x =>
                x.Merchant.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                x.Description.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                x.CategoryName.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (_selectedYear.HasValue)
            filtered = filtered.Where(x => x.TransactionDate.Year == _selectedYear.Value);

        if (_selectedMonth.HasValue)
            filtered = filtered.Where(x => x.TransactionDate.Month == _selectedMonth.Value);

        if (!string.IsNullOrWhiteSpace(_selectedDirection))
            filtered = filtered.Where(x => x.Direction == _selectedDirection);

        if (_selectedCategoryId.HasValue)
            filtered = filtered.Where(x => x.CategoryId == _selectedCategoryId.Value);

        filtered = _selectedSort switch
        {
            "oldest" => filtered.OrderBy(x => x.TransactionDate),
            "amount_desc" => filtered.OrderByDescending(x => x.Amount),
            "amount_asc" => filtered.OrderBy(x => x.Amount),
            _ => filtered.OrderByDescending(x => x.TransactionDate)
        };

        var list = filtered.ToList();

        IncomeLabel.Text = $"{list.Where(x => x.Direction == "Credit").Sum(x => x.Amount):N2} RON";
        ExpensesLabel.Text = $"{list.Where(x => x.Direction == "Debit").Sum(x => x.Amount):N2} RON";
        CountLabel.Text = list.Count.ToString();

        TransactionsList.ItemsSource = list;
    }
    private async Task ApplyFiltersWithLoadingAsync()
    {
        FilterLoadingOverlay.IsVisible = true;

        await Task.Delay(60); // lasă UI-ul să afișeze loader-ul

        try
        {
            var result = await Task.Run(() =>
            {
                var filtered = _allTransactions.AsEnumerable();

                var search = SearchEntry.Text?.Trim();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    filtered = filtered.Where(x =>
                        x.Merchant.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        x.Description.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        x.CategoryName.Contains(search, StringComparison.OrdinalIgnoreCase));
                }

                if (_selectedYear.HasValue)
                    filtered = filtered.Where(x => x.TransactionDate.Year == _selectedYear.Value);

                if (_selectedMonth.HasValue)
                    filtered = filtered.Where(x => x.TransactionDate.Month == _selectedMonth.Value);

                if (!string.IsNullOrWhiteSpace(_selectedDirection))
                    filtered = filtered.Where(x => x.Direction == _selectedDirection);

                if (_selectedCategoryId.HasValue)
                    filtered = filtered.Where(x => x.CategoryId == _selectedCategoryId.Value);

                filtered = _selectedSort switch
                {
                    "oldest" => filtered.OrderBy(x => x.TransactionDate),
                    "amount_desc" => filtered.OrderByDescending(x => x.Amount),
                    "amount_asc" => filtered.OrderBy(x => x.Amount),
                    _ => filtered.OrderByDescending(x => x.TransactionDate)
                };

                var list = filtered.ToList();

                var income = list.Where(x => x.Direction == "Credit").Sum(x => x.Amount);
                var expenses = list.Where(x => x.Direction == "Debit").Sum(x => x.Amount);

                return (list, income, expenses);
            });

            TransactionsList.ItemsSource = result.list;

            IncomeLabel.Text = $"{result.income:N2} RON";
            ExpensesLabel.Text = $"{result.expenses:N2} RON";
            CountLabel.Text = result.list.Count.ToString();
        }
        finally
        {
            FilterLoadingOverlay.IsVisible = false;
        }
    }

    private async void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        await ApplyFiltersWithLoadingAsync();
    }

    private async void OnYearTapped(object sender, TappedEventArgs e)
    {
        _activeFilter = "year";
        FilterTitleLabel.Text = "Selectează anul";
        FilterList.ItemsSource = _years;
        await OpenFilterSheetAsync();
    }

    private async void OnMonthTapped(object sender, TappedEventArgs e)
    {
        _activeFilter = "month";
        FilterTitleLabel.Text = "Selectează luna";
        FilterList.ItemsSource = _months;
        await OpenFilterSheetAsync();
    }

    private async void OnTypeTapped(object sender, TappedEventArgs e)
    {
        _activeFilter = "type";
        FilterTitleLabel.Text = "Selectează tipul";
        FilterList.ItemsSource = _types;
        await OpenFilterSheetAsync();
    }

    private async void OnCategoryTapped(object sender, TappedEventArgs e)
    {
        _activeFilter = "category";
        FilterTitleLabel.Text = "Selectează categoria";
        FilterList.ItemsSource = _categories;
        await OpenFilterSheetAsync();
    }

    private async void OnSortTapped(object sender, TappedEventArgs e)
    {
        _activeFilter = "sort";
        FilterTitleLabel.Text = "Selectează sortarea";
        FilterList.ItemsSource = _sorts;
        await OpenFilterSheetAsync();
    }

    private async void OnFilterSelected(object sender, EventArgs e)
    {
        if (sender is not Border border)
            return;

        switch (_activeFilter)
        {
            case "year":
                if (border.BindingContext is FilterOptionDto year)
                {
                    _selectedYear = year.Value;
                    SelectedYearLabel.Text = year.Name;
                }
                break;

            case "month":
                if (border.BindingContext is FilterOptionDto month)
                {
                    _selectedMonth = month.Value;
                    SelectedMonthLabel.Text = month.Name;
                }
                break;

            case "type":
                if (border.BindingContext is FilterOptionDto type)
                {
                    _selectedDirection = type.Value switch
                    {
                        1 => "Credit",
                        2 => "Debit",
                        _ => null
                    };

                    SelectedTypeLabel.Text = type.Name;
                }
                break;

            case "category":
                if (border.BindingContext is FilterOptionDto category)
                {
                    _selectedCategoryId = category.Value;
                    SelectedCategoryLabel.Text = category.Name;
                }
                break;

            case "sort":
                if (border.BindingContext is SortOptionDto sort)
                {
                    _selectedSort = sort.Value;
                    SelectedSortLabel.Text = sort.Name;
                }
                break;
        }

        await CloseFilterSheetAsync();
        await ApplyFiltersWithLoadingAsync();
    }

    private async Task OpenFilterSheetAsync()
    {
        FilterOverlay.IsVisible = true;
        FilterOverlay.Opacity = 0;
        FilterBottomSheet.TranslationY = 430;

        await Task.WhenAll(
            FilterOverlay.FadeTo(1, 120),
            FilterBottomSheet.TranslateTo(0, 0, 200, Easing.CubicOut)
        );
    }

    private async Task CloseFilterSheetAsync()
    {
        await Task.WhenAll(
            FilterOverlay.FadeTo(0, 120),
            FilterBottomSheet.TranslateTo(0, 430, 160, Easing.CubicIn)
        );

        FilterOverlay.IsVisible = false;
    }

    private async void OnCloseFilterClicked(object sender, EventArgs e)
    {
        await CloseFilterSheetAsync();
    }
}