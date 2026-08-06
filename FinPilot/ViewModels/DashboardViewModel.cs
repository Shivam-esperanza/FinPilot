using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        private Guid _currentUserId;

        [ObservableProperty]
        private DashboardSummary? _summary;

        [ObservableProperty]
        private bool _isRefreshing;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        // Dependency Injection container hydrates our service layers automatically
        public DashboardViewModel(IDashboardService dashboardService,
    ILoanTrackingService loanTrackingService,
    INotificationManagerService notificationManager,
    ISyncRepository syncRepositor)
        {
            _dashboardService = dashboardService;
        }

        // Structural entry gateway initialization parameters routing from login channels
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
                // 1. Catch up loan amortization decaying logic
                await _loanTrackingService.AutoUpdateBalancesAsync(_currentUserId);

                // 2. Run background audit sweep to populate smart alerts (Feature 8)
                await _notificationManager.RunBackgroundAuditAsync(_currentUserId);

                // 3. Load aggregated summary maps into the UI
                Summary = await _dashboardService.GetDashboardSummaryAsync(_currentUserId);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to synchronize dashboard metrics: {ex.Message}";
            }
            finally
            {
                IsRefreshing = false;
            }
        }
    }
}
