using Microsoft.Maui.Controls;
using FinPilot.ViewModels;

namespace FinPilot.Views
{
    public partial class RegisterPage : ContentPage
    {
        public RegisterPage(RegisterViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
