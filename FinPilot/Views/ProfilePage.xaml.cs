using Microsoft.Maui.Controls;
using FinPilot.ViewModels;

namespace FinPilot.Views
{
    public partial class ProfilePage : ContentPage
    {
        private readonly ProfileViewModel _viewModel;

        public ProfilePage(ProfileViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadProfileAsync();
        }
    }
}
