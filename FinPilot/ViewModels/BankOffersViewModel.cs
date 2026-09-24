using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinPilot.Interfaces;
using FinPilot.Models;

namespace FinPilot.ViewModels
{
    public partial class BankOffersViewModel : ObservableObject
    {
        private readonly IBankOfferService _offerService;

        [ObservableProperty]
        private ObservableCollection<Offer> _offers = new();

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private string _syncStatusMessage = "Offers auto-synced live from internet.";

        [ObservableProperty]
        private string _lastSyncedTimeText = $"Updated at {DateTime.Now:hh:mm tt}";

        public BankOffersViewModel(IBankOfferService offerService)
        {
            _offerService = offerService;
        }

        [RelayCommand]
        public async Task LoadOffersAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                var list = await _offerService.GetActiveMarketOffersAsync();
                Offers = new ObservableCollection<Offer>(list);
                LastSyncedTimeText = $"Updated at {DateTime.Now:hh:mm tt}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading bank offers: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task RefreshOffersAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;
                SyncStatusMessage = "Fetching real-time bank offers from internet...";

                var list = await _offerService.SyncLatestOffersFromInternetAsync();
                Offers = new ObservableCollection<Offer>(list);
                LastSyncedTimeText = $"Updated live at {DateTime.Now:hh:mm tt}";
                SyncStatusMessage = "Bank offers successfully refreshed from internet.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error syncing offers from internet: {ex.Message}";
                SyncStatusMessage = "Network sync offline. Displaying cached local offers.";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
