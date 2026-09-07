using CommunityToolkit.Maui;
using FinPilot.Database;
using FinPilot.Interfaces;
using FinPilot.Services;
using FinPilot.ViewModels;
using FinPilot.Views;
using Microsoft.EntityFrameworkCore;
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

            // Database Context (Transient to allow independent DbContext instances per page/service in MAUI)
            builder.Services.AddDbContext<AppDbContext>(ServiceLifetime.Transient);

            // Core Application Services & Interfaces
            builder.Services.AddSingleton<IUserSessionService, UserSessionService>();
            builder.Services.AddTransient<IAuthenticationService, AuthenticationService>();
            builder.Services.AddTransient<IAccountService, AccountService>();
            builder.Services.AddTransient<ICreditCardService, CreditCardService>();
            builder.Services.AddTransient<ILoanTrackingService, LoanTrackingService>();
            builder.Services.AddTransient<ITransactionService, TransactionService>();
            builder.Services.AddTransient<IDashboardService, DashboardService>();
            builder.Services.AddTransient<IBankOfferService, BankOfferService>();
            builder.Services.AddTransient<IRecommendationEngine, RecommendationEngine>();
            builder.Services.AddTransient<IAssistantService, AssistantService>();
            builder.Services.AddTransient<INotificationManagerService, NotificationManagerService>();
            builder.Services.AddTransient<ISyncRepository, CloudSyncRepository>();

            // ViewModels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<AccountsViewModel>();
            builder.Services.AddTransient<CreditCardManagerViewModel>();
            builder.Services.AddTransient<LoansViewModel>();
            builder.Services.AddTransient<TransactionsViewModel>();
            builder.Services.AddTransient<PurchaseAdvisorViewModel>();
            builder.Services.AddTransient<BankOffersViewModel>();
            builder.Services.AddTransient<NotificationsViewModel>();
            builder.Services.AddTransient<ProfileViewModel>();

            // Views
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<AccountsPage>();
            builder.Services.AddTransient<CreditCardManagerPage>();
            builder.Services.AddTransient<LoansPage>();
            builder.Services.AddTransient<TransactionsPage>();
            builder.Services.AddTransient<PurchaseAdvisorPage>();
            builder.Services.AddTransient<BankOffersPage>();
            builder.Services.AddTransient<NotificationsPage>();
            builder.Services.AddTransient<ProfilePage>();

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

                // Self-healing migration for existing databases missing the PasswordHash column
                try
                {
                    context.Database.ExecuteSqlRaw("ALTER TABLE Users ADD COLUMN PasswordHash TEXT NOT NULL DEFAULT '';");
                }
                catch
                {
                    // Column already exists or table created cleanly by EnsureCreated
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"An error occurred while initializing the database: {ex.Message}");
            }
        }
    }
}
