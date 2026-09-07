using Microsoft.Maui.Controls;
using FinPilot.ViewModels;

namespace FinPilot.Views
{
    public partial class TransactionsPage : ContentPage
    {
        private readonly TransactionsViewModel _viewModel;

        public TransactionsPage(TransactionsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadTransactionsAsync();
        }
    }
}
