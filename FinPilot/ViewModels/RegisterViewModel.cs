using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinPilot.Interfaces;
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace FinPilot.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly IAuthenticationService _authService;

        [ObservableProperty]
        private string _fullName = string.Empty;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _confirmPassword = string.Empty;

        [ObservableProperty]
        private string _phoneNumber = string.Empty;

        [ObservableProperty]
        private string _monthlyIncomeText = string.Empty;

        [ObservableProperty]
        private string _creditScoreText = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private string _successMessage = string.Empty;

        public RegisterViewModel(IAuthenticationService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        private async Task RegisterAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;

                // 1. Full Name Validation
                if (string.IsNullOrWhiteSpace(FullName))
                {
                    ErrorMessage = "Full Name is required.";
                    return;
                }

                // 2. Email Format Validation
                if (string.IsNullOrWhiteSpace(Email) || !Email.Contains("@") || !Email.Contains("."))
                {
                    ErrorMessage = "A valid email address is required.";
                    return;
                }

                // 3. Password Length Validation
                if (string.IsNullOrWhiteSpace(Password) || Password.Length < 6)
                {
                    ErrorMessage = "Password is required and must be at least 6 characters.";
                    return;
                }

                // 4. Confirm Password Matching
                if (Password != ConfirmPassword)
                {
                    ErrorMessage = "Password and Confirm Password do not match.";
                    return;
                }

                // 5. Phone Number Validation
                if (string.IsNullOrWhiteSpace(PhoneNumber) || PhoneNumber.Trim().Length < 10)
                {
                    ErrorMessage = "Please enter a valid phone number (at least 10 digits).";
                    return;
                }

                // 6. Monthly Income Validation
                if (!decimal.TryParse(MonthlyIncomeText, out decimal income) || income < 0)
                {
                    ErrorMessage = "Please enter a valid non-negative monthly income (e.g., 75000).";
                    return;
                }

                // 7. Credit Score Validation
                if (!int.TryParse(CreditScoreText, out int creditScore) || creditScore < 300 || creditScore > 900)
                {
                    ErrorMessage = "Please enter a valid CIBIL credit score (between 300 and 900).";
                    return;
                }

                // Call Authentication Service to register user
                var user = await _authService.RegisterWithEmailAsync(
                    FullName.Trim(),
                    Email.Trim(),
                    Password,
                    PhoneNumber.Trim(),
                    income,
                    creditScore);

                if (user == null)
                {
                    ErrorMessage = "Registration failed. An account with this email address already exists.";
                    return;
                }

                SuccessMessage = "Registration successful! Redirecting to login...";
                await Task.Delay(1000);

                // Navigate back to Login page
                if (App.Current?.Windows[0].Page is NavigationPage navPage)
                {
                    await navPage.PopAsync();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An unexpected error occurred during registration: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task NavigateToLoginAsync()
        {
            if (App.Current?.Windows[0].Page is NavigationPage navPage)
            {
                await navPage.PopAsync();
            }
        }
    }
}
