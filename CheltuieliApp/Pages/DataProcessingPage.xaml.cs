namespace CheltuieliApp.Pages;

public partial class DataProcessingPage : ContentPage
{
    public DataProcessingPage()
    {
        InitializeComponent();
    }

    private async void OnBackTapped(object sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }
}