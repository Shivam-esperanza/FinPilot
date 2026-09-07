using Microsoft.Maui.Controls;
using FinPilot.ViewModels;

namespace FinPilot.Views
{
    public partial class BankOffersPage : ContentPage
    {
        private readonly BankOffersViewModel _viewModel;

        public BankOffersPage(BankOffersViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadOffersAsync();
        }
    }
}
