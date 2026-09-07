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
    }
}
