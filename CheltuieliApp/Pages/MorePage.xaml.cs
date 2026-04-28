using Microsoft.Extensions.DependencyInjection;

namespace CheltuieliApp.Pages;

public partial class MorePage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;

    public MorePage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    private async void OnCategoriesTapped(object sender, TappedEventArgs e)
    {
        var page = _serviceProvider.GetRequiredService<CategoriesPage>();
        await Navigation.PushAsync(page);
    }

    private async void OnDataProcessingTapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new DataProcessingPage());
    }

    private async void OnHelpTapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new HelpPage());
    }

    private async void OnInfoTapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new InfoPage());
    }
    private async void OnBackupTapped(object sender, TappedEventArgs e)
    {
        var page = _serviceProvider.GetRequiredService<BackupPage>();
        await Navigation.PushAsync(page);
    }
}