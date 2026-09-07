using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinPilot.Interfaces;
using FinPilot.Models;

namespace FinPilot.ViewModels
{
    public partial class NotificationsViewModel : ObservableObject
    {
        private readonly INotificationManagerService _notificationService;
        private readonly IUserSessionService _sessionService;

        [ObservableProperty]
        private ObservableCollection<Notification> _notifications = new();

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public NotificationsViewModel(INotificationManagerService notificationService, IUserSessionService sessionService)
        {
            _notificationService = notificationService;
            _sessionService = sessionService;
        }

        [RelayCommand]
        public async Task LoadNotificationsAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                Guid userId = _sessionService.CurrentUserId ?? Guid.Empty;
                if (userId == Guid.Empty) return;

                await _notificationService.RunBackgroundAuditAsync(userId);
                var items = await _notificationService.GetUserNotificationsAsync(userId);
                Notifications = new ObservableCollection<Notification>(items);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading notifications: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task MarkAsReadAsync(Notification notification)
        {
            if (notification == null) return;
            await _notificationService.MarkAsReadAsync(notification.Id);
            notification.IsRead = true;
            await LoadNotificationsAsync();
        }
    }
}
