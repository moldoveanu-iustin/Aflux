using Microsoft.Extensions.Logging;
using CheltuieliApp.Data;
using CheltuieliApp.Services;

namespace CheltuieliApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "cheltuieli.db3");
            builder.Services.AddSingleton(new AppDatabase(dbPath));
            builder.Services.AddSingleton<ImportService>();


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
