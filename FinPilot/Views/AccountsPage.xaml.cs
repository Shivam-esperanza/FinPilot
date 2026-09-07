using Microsoft.Maui.Controls;
using FinPilot.ViewModels;

namespace FinPilot.Views
{
    public partial class AccountsPage : ContentPage
    {
        private readonly AccountsViewModel _viewModel;

        public AccountsPage(AccountsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadAccountsAsync();
        }
    }
}
