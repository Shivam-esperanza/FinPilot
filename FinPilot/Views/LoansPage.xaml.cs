using Microsoft.Maui.Controls;
using FinPilot.ViewModels;

namespace FinPilot.Views
{
    public partial class LoansPage : ContentPage
    {
        private readonly LoansViewModel _viewModel;

        public LoansPage(LoansViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadLoansAsync();
        }
    }
}
