using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls;
using FinPilot.ViewModels;

namespace FinPilot.Views
{
    public partial class LoginPage : ContentPage
    {
        // Constructor Injection allows the MAUI DI engine to pass our compiled viewmodel
        public LoginPage(LoginViewModel viewModel)
        {
            InitializeComponent();

            // Setting the BindingContext wires up our XAML {Binding} tags completely
            BindingContext = viewModel;
        }
    }
}

