using System.Globalization;
using CheltuieliApp.DTOs;
using CheltuieliApp.Models;
using CheltuieliApp.Services;
using Microsoft.Maui.Controls.Shapes;

namespace CheltuieliApp.Pages;

public partial class DashboardPage : ContentPage
{
    private readonly ImportService _importService;
    private bool _isLoading;
    private DateTime _selectedMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    public DashboardPage(ImportService importService)
    {
        InitializeComponent();
        _importService = importService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDashboardAsync();
    }
    private async void OnPreviousMonthClicked(object sender, EventArgs e)
    {
        if (_isLoading) return;
        _isLoading = true;        
        _selectedMonth = _selectedMonth.AddMonths(-1);
        await LoadDashboardAsync();
        _isLoading = false;
    }

    private async void OnNextMonthClicked(object sender, EventArgs e)
    {
        if (_isLoading) return;
        _isLoading = true;
        _selectedMonth = _selectedMonth.AddMonths(1);
        await LoadDashboardAsync();
        _isLoading = false;
    }

    private async Task LoadDashboardAsync()
    {
        var selectedYear = _selectedMonth.Year;
        var selectedMonth = _selectedMonth.Month;

        var imports = await _importService.GetImportsForMonthAsync(selectedYear, selectedMonth);
        var summary = await _importService.GetMonthlySummaryAsync(selectedYear, selectedMonth);
        var transactions = await _importService.GetTransactionsForMonthAsync(selectedYear, selectedMonth);

        LoadSummary(summary);

        var monthName = CultureInfo
            .GetCultureInfo("ro-RO")
            .DateTimeFormat
            .GetMonthName(selectedMonth);

        CurrentMonthLabel.Text = $"{char.ToUpper(monthName[0])}{monthName[1..]} {selectedYear}";

        LoadCalendar(selectedYear, selectedMonth, imports, transactions);
    }

    private void LoadSummary((decimal Income, decimal Expenses, int Count) summary)
    {
        IncomeLabel.Text = $"{summary.Income:N2} RON";
        ExpensesLabel.Text = $"{summary.Expenses:N2} RON";
        TransactionsCountLabel.Text = summary.Count.ToString();
    }

    private void LoadCalendar(int year, int month, List<StatementImportEntity> imports, List<TransactionEntity> transactions)
    {
        var days = BuildCalendarDays(year, month, imports, transactions);

        CalendarGrid.Children.Clear();
        CalendarGrid.RowDefinitions.Clear();

        var rows = (int)Math.Ceiling(days.Count / 7.0);

        for (int i = 0; i < rows; i++)
            CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        for (int i = 0; i < days.Count; i++)
        {
            var day = days[i];

            var row = i / 7;
            var col = i % 7;

            var dayView = CreateDayView(day);

            Grid.SetRow(dayView, row);
            Grid.SetColumn(dayView, col);

            CalendarGrid.Children.Add(dayView);
        }
    }
    private View CreateDayView(DashboardDayDto day)
    {
        var hasAnyBank = day.HasBt || day.HasBrd || day.HasRaiffeisen;

        var dayLabel = new Label
        {
            Text = day.DayNumber.ToString(),
            HorizontalTextAlignment = TextAlignment.Center,
            FontAttributes = day.IsToday ? FontAttributes.Bold : FontAttributes.None,
            FontSize = 14,
            TextColor = day.IsCurrentMonth ? Color.FromArgb("#111827") : Color.FromArgb("#CBD5E1")
        };

        var dots = new HorizontalStackLayout
        {
            Spacing = 3,
            HorizontalOptions = LayoutOptions.Center,
            HeightRequest = 10,
            Opacity = day.IsCurrentMonth ? 1 : 0.25
        };

        if (day.HasBt)
        {
            dots.Children.Add(new BoxView
            {
                WidthRequest = 6,
                HeightRequest = 6,
                Color = Color.FromArgb("#38BDF8")
            });
        }

        if (day.HasBrd)
        {
            dots.Children.Add(new BoxView
            {
                WidthRequest = 6,
                HeightRequest = 6,
                Color = Color.FromArgb("#EF4444")
            });
        }

        if (day.HasRaiffeisen)
        {
            dots.Children.Add(new BoxView
            {
                WidthRequest = 6,
                HeightRequest = 6,
                Color = Color.FromArgb("#FACC15")
            });
        }

        var stack = new VerticalStackLayout
        {
            Spacing = 4,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Children =
        {
            dayLabel,
            dots
        }
        };

        var border = new Border
        {
            Padding = 6,
            StrokeShape = new RoundRectangle
            {
                CornerRadius = 16
            },
            StrokeThickness = day.IsToday ? 2 : 0,
            Stroke = day.IsToday ? Color.FromArgb("#3F51B5") : Colors.Transparent,
            BackgroundColor = day.HasTransactions && day.IsCurrentMonth ? Color.FromArgb("#F1F5F9") : Colors.Transparent,
            MinimumHeightRequest = 58,
            Content = stack
        };

        border.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () => await OnDaySelectedAsync(day.Date))
        });

        return border;
    }

    private static List<DashboardDayDto> BuildCalendarDays( int year, int month, List<StatementImportEntity> imports, List<TransactionEntity> transactions)
    {
        var result = new List<DashboardDayDto>();

        var firstDayOfMonth = new DateTime(year, month, 1);
        var daysInMonth = DateTime.DaysInMonth(year, month);

        var firstDayOfWeek = GetMondayBasedDayOfWeek(firstDayOfMonth.DayOfWeek);

        for (var i = 1; i < firstDayOfWeek; i++)
        {
            result.Add(new DashboardDayDto
            {
                Date = firstDayOfMonth.AddDays(-firstDayOfWeek + i),
                IsCurrentMonth = false
            });
        }

        for (var day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(year, month, day);

            var importsForDay = imports
                .Where(x => x.PeriodStart.Date <= date.Date && x.PeriodEnd.Date >= date.Date)
                .ToList();

            var transactionsForDay = transactions.Any(t => t.TransactionDate.Date == date.Date);

            result.Add(new DashboardDayDto
            {
                Date = date,
                IsCurrentMonth = true,
                HasBt = importsForDay.Any(x => x.Bank.Equals("BT", StringComparison.OrdinalIgnoreCase)),
                HasBrd = importsForDay.Any(x => x.Bank.Equals("BRD", StringComparison.OrdinalIgnoreCase)),
                HasRaiffeisen = importsForDay.Any(x => x.Bank.Contains("Raiffeisen", StringComparison.OrdinalIgnoreCase)),
                HasTransactions = transactionsForDay
            });
        }

        while (result.Count % 7 != 0)
        {
            var nextDate = result.Last().Date.AddDays(1);

            result.Add(new DashboardDayDto
            {
                Date = nextDate,
                IsCurrentMonth = false
            });
        }

        return result;
    }

    private static int GetMondayBasedDayOfWeek(DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Monday => 1,
            DayOfWeek.Tuesday => 2,
            DayOfWeek.Wednesday => 3,
            DayOfWeek.Thursday => 4,
            DayOfWeek.Friday => 5,
            DayOfWeek.Saturday => 6,
            DayOfWeek.Sunday => 7,
            _ => 1
        };
    }
    private async Task OnDaySelectedAsync(DateTime date)
    {
        var transactions = await _importService.GetTransactionsForDayAsync(date);

        if (!transactions.Any())
        {
            await DisplayAlertAsync(
                $"{date:dd.MM.yyyy}",
                "Nu există tranzacții în această zi.",
                "OK");

            return;
        }

        var totalIncome = transactions
            .Where(x => x.Direction == "Credit")
            .Sum(x => x.Amount);

        var totalExpenses = transactions
            .Where(x => x.Direction == "Debit")
            .Sum(x => x.Amount);

        var lines = transactions.Select(x =>
        {
            var sign = x.Direction == "Credit" ? "+" : "-";
            return $"{x.Bank} | {x.Merchant} | {sign}{x.Amount:N2} RON";
        });

        var message =
            $"Venituri: {totalIncome:N2} RON\n" +
            $"Cheltuieli: {totalExpenses:N2} RON\n\n" +
            string.Join("\n", lines);

        await DisplayAlertAsync(
            $"{date:dd.MM.yyyy}",
            message,
            "OK");
    }

    private async void OnCalendarInfoClicked(object sender, EventArgs e)
    {
        InfoOverlay.IsVisible = true;

        InfoBottomSheet.TranslationY = 430;
        InfoOverlay.Opacity = 0;

        await Task.WhenAll(
            InfoOverlay.FadeTo(1, 150),
            InfoBottomSheet.TranslateTo(0, 0, 220, Easing.CubicOut)
        );
    }

    private async void OnCloseCalendarInfoClicked(object sender, EventArgs e)
    {
        await Task.WhenAll(
            InfoOverlay.FadeTo(0, 120),
            InfoBottomSheet.TranslateTo(0, 430, 180, Easing.CubicIn)
        );

        InfoOverlay.IsVisible = false;
    }
}