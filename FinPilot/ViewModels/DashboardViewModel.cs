using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using FinPilot.Interfaces;
using FinPilot.Models;

namespace FinPilot.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILoanTrackingService _loanTrackingService;
        private readonly INotificationManagerService _notificationManager;
        private readonly ISyncRepository _syncRepository;
        private readonly IUserSessionService _sessionService;
        private readonly IAuthenticationService _authService;
        private Guid _currentUserId;

        [ObservableProperty]
        private DashboardSummary? _summary;

        [ObservableProperty]
        private bool _isRefreshing;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public DashboardViewModel(
            IDashboardService dashboardService,
            ILoanTrackingService loanTrackingService,
            INotificationManagerService notificationManager,
            ISyncRepository syncRepository,
            IUserSessionService sessionService,
            IAuthenticationService authService)
        {
            _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
            _loanTrackingService = loanTrackingService ?? throw new ArgumentNullException(nameof(loanTrackingService));
            _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
            _syncRepository = syncRepository ?? throw new ArgumentNullException(nameof(syncRepository));
            _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        public void Initialize(Guid userId)
        {
            _currentUserId = userId;
        }

        [RelayCommand]
        public async Task LoadDashboardDataAsync()
        {
            if (IsRefreshing) return;

            try
            {
                IsRefreshing = true;
                ErrorMessage = string.Empty;

                Guid targetUserId = _currentUserId != Guid.Empty
                    ? _currentUserId
                    : (_sessionService?.CurrentUserId ?? Guid.Empty);

                if (targetUserId == Guid.Empty)
                {
                    var currentUser = await _authService.GetCurrentCurrentUserAsync();
                    if (currentUser != null)
                    {
                        targetUserId = currentUser.Id;
                    }
                }

                if (targetUserId == Guid.Empty)
                {
                    ErrorMessage = "User session is invalid. Please log in again.";
                    return;
                }

                if (_loanTrackingService != null)
                {
                    await _loanTrackingService.AutoUpdateBalancesAsync(targetUserId);
                }

                if (_notificationManager != null)
                {
                    await _notificationManager.RunBackgroundAuditAsync(targetUserId);
                }

                if (_dashboardService != null)
                {
                    Summary = await _dashboardService.GetDashboardSummaryAsync(targetUserId);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to synchronize dashboard metrics: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[DashboardViewModel Exception] {ex}");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private async Task PushPageAsync<TPage>() where TPage : Page
        {
            var page = App.Current?.Windows[0].Page?.Handler?.MauiContext?.Services.GetService<TPage>();
            if (page != null && App.Current?.Windows[0].Page is NavigationPage navPage)
            {
                await navPage.PushAsync(page);
            }
        }

        [RelayCommand]
        private async Task NavigateToAccountsAsync() => await PushPageAsync<Views.AccountsPage>();

        [RelayCommand]
        private async Task NavigateToCardsAsync() => await PushPageAsync<Views.CreditCardManagerPage>();

        [RelayCommand]
        private async Task NavigateToLoansAsync() => await PushPageAsync<Views.LoansPage>();

        [RelayCommand]
        private async Task NavigateToTransactionsAsync() => await PushPageAsync<Views.TransactionsPage>();

        [RelayCommand]
        private async Task NavigateToAdvisorAsync() => await PushPageAsync<Views.PurchaseAdvisorPage>();

        [RelayCommand]
        private async Task NavigateToOffersAsync() => await PushPageAsync<Views.BankOffersPage>();

        [RelayCommand]
        private async Task NavigateToNotificationsAsync() => await PushPageAsync<Views.NotificationsPage>();

        [RelayCommand]
        private async Task NavigateToProfileAsync() => await PushPageAsync<Views.ProfilePage>();

        [RelayCommand]
        private async Task LogoutAsync()
        {
            await _authService.LogoutAsync();
            if (App.Current?.Windows[0].Page is NavigationPage navPage)
            {
                await navPage.PopToRootAsync();
            }
        }
    }
}
