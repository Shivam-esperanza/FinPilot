using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using FinPilot.Views;

namespace FinPilot
{
    public partial class App : Application
    {
        public App(LoginPage loginPage)
        {
            InitializeComponent();

            //set the absolute root window navigate layout pointer
            MainPage = new NavigationPage(loginPage); 
        }
    }
}