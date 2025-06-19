using Microsoft.Extensions.Logging;
using MauiExamApp.Services; // Tilføj denne
using MauiExamApp.ViewModels; // Tilføj denne
using MauiExamApp.Views; // Tilføj denne

namespace MauiExamApp
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

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            // Registrer Database Service som Singleton
            // (Vi vil kun have én instans af databasen i hele appens levetid)
            builder.Services.AddSingleton<DatabaseService>();

            // Registrer Views og ViewModels som Transient
            // (Vi vil have en ny instans hver gang vi navigerer til en side)
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<DashboardView>();

            builder.Services.AddTransient<ExamViewModel>();
            builder.Services.AddTransient<ExamView>();

            builder.Services.AddTransient<HistoryViewModel>();
            builder.Services.AddTransient<HistoryView>();

            return builder.Build();
        }
    }
}