using Microsoft.Maui.Controls;
using FinPilot.ViewModels;

namespace FinPilot.Views
{
    public partial class NotificationsPage : ContentPage
    {
        private readonly NotificationsViewModel _viewModel;

        public NotificationsPage(NotificationsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadNotificationsAsync();
        }
    }
}
