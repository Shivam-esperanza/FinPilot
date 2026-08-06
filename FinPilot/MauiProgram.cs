using CommunityToolkit.Maui;
using FinPilot.Database;
using FinPilot.Interfaces;
using FinPilot.Services;
using FinPilot.Views;
using FinPilot.ViewModels;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FinPilot
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            builder.Services.AddDbContext<AppDbContext>();
            builder.Services.AddScoped<IAuthenticationService, MockAuthService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddScoped<ILoanTrackingService, LoanTrackingService>();
            builder.Services.AddScoped<ICreditCardService, CreditCardService>();
            builder.Services.AddScoped<INotificationManagerService, NotificationManagerService>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<Views.DashboardPage>();
            builder.Services.AddScoped<IBankOfferService, BankOfferService>();
            builder.Services.AddScoped<IRecommendationEngine, RecommendationEngine>();
            builder.Services.AddScoped<INotificationManagerService, NotificationManagerService>();
            builder.Services.AddScoped<ISyncRepository, CloudSyncRepository>();



#if DEBUG
            builder.Logging.AddDebug();
#endif
            var app = builder.Build();

            InitializeDatabase(app);
            return app;
        }

        private static void InitializeDatabase(MauiApp app)
        {
            try
            {
                using var scope = app.Services.CreateScope();
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<AppDbContext>();
                context.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"An error occurred while initializing the database: {ex.Message}");
            }
        }
    }
}
