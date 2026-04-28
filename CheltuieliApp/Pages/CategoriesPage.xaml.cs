using CheltuieliApp.DTOs;
using CheltuieliApp.Models;
using CheltuieliApp.Services;

namespace CheltuieliApp.Pages;

public partial class CategoriesPage : ContentPage
{
    private readonly CategoryService _categoryService;
    private string _selectedColor = "#3F51B5";
    private string _selectedType = "Expense";
    private bool _defaultsEnsured;

    public CategoriesPage(CategoryService categoryService)
    {
        InitializeComponent();
        _categoryService = categoryService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await LoadCategoriesAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Eroare categorii",
                ex.ToString(),
                "OK");
        }
    }

    private async Task LoadCategoriesAsync()
    {
        LoadingCategoriesLabel.IsVisible = true;
        CategoriesList.IsVisible = false;

        if (!_defaultsEnsured)
        {
            await _categoryService.EnsureDefaultsAsync();
            _defaultsEnsured = true;
        }

        var categories = await _categoryService.GetCategoriesAsync();

        CategoriesList.ItemsSource = categories;

        LoadingCategoriesLabel.IsVisible = false;
        CategoriesList.IsVisible = true;
    }

    private async void OnAddCategoryClicked(object sender, EventArgs e)
    {
        CategoryNameEntry.Text = "";
        _selectedType = "Expense";
        UpdateTypeButtonsUi();
        _selectedColor = "#3F51B5";

        await OpenAddCategorySheetAsync();
    }

    private async void OnCloseAddCategoryClicked(object sender, EventArgs e)
    {
        await CloseAddCategorySheetAsync();
    }

    private void OnColorTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not string color)
            return;

        _selectedColor = color;
        UpdateSelectedColorUi();
    }
    private void UpdateSelectedColorUi()
    {
        var colors = new[]
        {
        ColorBlue,
        ColorGreen,
        ColorRed,
        ColorOrange,
        ColorPurple
    };

        foreach (var item in colors)
        {
            item.StrokeThickness = 0;
            item.Scale = 1;
        }

        Border selected = _selectedColor switch
        {
            "#3F51B5" => ColorBlue,
            "#22C55E" => ColorGreen,
            "#EF4444" => ColorRed,
            "#F59E0B" => ColorOrange,
            "#8B5CF6" => ColorPurple,
            _ => ColorBlue
        };

        selected.Stroke = Colors.Black;
        selected.StrokeThickness = 3;
        selected.Scale = 1.04;
    }

    private async void OnSaveCategoryClicked(object sender, EventArgs e)
    {
        var name = CategoryNameEntry.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(name))
        {
            await DisplayAlertAsync("Nume lipsă", "Introdu un nume pentru categorie.", "OK");
            return;
        }

        var type = _selectedType;

        await _categoryService.AddCategoryAsync(name, type, _selectedColor);

        await CloseAddCategorySheetAsync();
        await LoadCategoriesAsync();

        CategoryNameEntry.Unfocus();
        await Task.Delay(100);
    }

    private async void OnDeleteCategoryClicked(object sender, EventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.BindingContext is not CategoryEntity category)
            return;

        var used = await _categoryService.IsCategoryUsedAsync(category.Id);

        var message = used
            ? "Această categorie este folosită deja de tranzacții. Dacă o ștergi, ea va fi ascunsă pentru importurile viitoare, dar tranzacțiile vechi își vor păstra numele categoriei."
            : "Sigur vrei să ștergi această categorie?";

        var confirm = await DisplayAlertAsync(
            "Ștergere categorie",
            message,
            "Șterge",
            "Anulează");

        if (!confirm)
            return;

        await _categoryService.DeleteCategoryAsync(category.Id);
        await LoadCategoriesAsync();
    }

    private async Task OpenAddCategorySheetAsync()
    {
        AddCategoryOverlay.IsVisible = true;

        AddCategoryBottomSheet.TranslationY = 350;
        AddCategoryOverlay.Opacity = 0;
        await AddCategoryBottomSheet.TranslateTo(0, 0, 200, Easing.CubicOut);

        await Task.WhenAll(
            AddCategoryOverlay.FadeTo(1, 150),
            AddCategoryBottomSheet.TranslateTo(0, 0, 220, Easing.CubicOut)
        );
    }

    private async Task CloseAddCategorySheetAsync()
    {
        await Task.WhenAll(
            AddCategoryOverlay.FadeTo(0, 120),
            AddCategoryBottomSheet.TranslateTo(0, 390, 180, Easing.CubicIn)
        );

        AddCategoryOverlay.IsVisible = false;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
    private void OnTypeSelected(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string type)
        {
            _selectedType = type;
            UpdateTypeButtonsUi();
        }
    }

    private void UpdateTypeButtonsUi()
    {
        SetTypeButtonStyle(ExpenseTypeButton, _selectedType == "Expense");
        SetTypeButtonStyle(IncomeTypeButton, _selectedType == "Income");
        SetTypeButtonStyle(BothTypeButton, _selectedType == "Both");
    }

    private static void SetTypeButtonStyle(Button button, bool selected)
    {
        button.BackgroundColor = selected
            ? Color.FromArgb("#3F51B5")
            : Color.FromArgb("#EEF2FF");

        button.TextColor = selected
            ? Colors.White
            : Color.FromArgb("#3F51B5");
    }
}