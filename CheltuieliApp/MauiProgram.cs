using CheltuieliApp.Data;
using CheltuieliApp.Pages;
using CheltuieliApp.Services;
using Microsoft.Extensions.Logging;

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
            builder.Services.AddSingleton<CategoryService>();
            builder.Services.AddTransient<CategoriesPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
