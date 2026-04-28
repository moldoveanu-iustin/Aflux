using CheltuieliApp.Services;

namespace CheltuieliApp.Pages;

public partial class BackupPage : ContentPage
{
    private readonly BackupService _backupService;

    public BackupPage(BackupService backupService)
    {
        InitializeComponent();
        _backupService = backupService;
    }

    private async void OnBackTapped(object sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnExportClicked(object sender, EventArgs e)
    {
        try
        {
            var path = await _backupService.ExportBackupAsync();

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Export backup Aflux",
                File = new ShareFile(path)
            });
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Eroare export", ex.Message, "OK");
        }
    }

    private async void OnImportClicked(object sender, EventArgs e)
    {
        try
        {
            var file = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Alege backup-ul JSON"
            });

            if (file == null)
                return;

            var replace = await DisplayAlertAsync(
                "Mod import",
                "Vrei să înlocuiești datele existente? Dacă alegi Nu, datele din backup se vor adăuga peste cele actuale.",
                "Înlocuiește",
                "Adaugă peste");

            await _backupService.ImportBackupAsync(file.FullPath, replace);

            await DisplayAlertAsync("Succes", "Backup-ul a fost importat.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Eroare import", ex.Message, "OK");
        }
    }
}