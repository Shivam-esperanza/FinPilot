using Microsoft.Maui.Controls;
using FinPilot.ViewModels;

namespace FinPilot.Views
{
    public partial class CreditCardManagerPage : ContentPage
    {
        private readonly CreditCardManagerViewModel _viewModel;

        public CreditCardManagerPage(CreditCardManagerViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadCardsAsync();
        }
    }
}
