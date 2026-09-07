using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using FinPilot.Database;
using FinPilot.Interfaces;
using FinPilot.Models;
using FinPilot.Services;

namespace FinPilot.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserSessionService _sessionService;

        [ObservableProperty]
        private string _fullName = string.Empty;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _phoneNumber = string.Empty;

        [ObservableProperty]
        private string _monthlyIncomeText = string.Empty;

        [ObservableProperty]
        private string _creditScoreText = string.Empty;

        [ObservableProperty]
        private string _currentPassword = string.Empty;

        [ObservableProperty]
        private string _newPassword = string.Empty;

        [ObservableProperty]
        private string _confirmNewPassword = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private string _successMessage = string.Empty;

        public ProfileViewModel(AppDbContext dbContext, IUserSessionService sessionService)
        {
            _dbContext = dbContext;
            _sessionService = sessionService;
        }

        [RelayCommand]
        public async Task LoadProfileAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;

                Guid userId = _sessionService.CurrentUserId ?? Guid.Empty;
                if (userId == Guid.Empty) return;

                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user != null)
                {
                    FullName = user.FullName;
                    Email = user.Email;
                    PhoneNumber = user.PhoneNumber;
                    MonthlyIncomeText = user.MonthlyIncome.ToString("F0");
                    CreditScoreText = user.CreditScore.ToString();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading profile: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task UpdateProfileAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;

                Guid userId = _sessionService.CurrentUserId ?? Guid.Empty;
                if (userId == Guid.Empty) return;

                if (string.IsNullOrWhiteSpace(FullName))
                {
                    ErrorMessage = "Full Name is required.";
                    return;
                }

                if (!decimal.TryParse(MonthlyIncomeText, out decimal income) || income < 0)
                {
                    ErrorMessage = "Please enter a valid monthly income.";
                    return;
                }

                if (!int.TryParse(CreditScoreText, out int score) || score < 300 || score > 900)
                {
                    ErrorMessage = "Please enter a valid CIBIL score (300-900).";
                    return;
                }

                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    ErrorMessage = "User profile not found.";
                    return;
                }

                user.FullName = FullName.Trim();
                user.PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? user.PhoneNumber : PhoneNumber.Trim();
                user.MonthlyIncome = income;
                user.CreditScore = score;

                // Process Password Change if supplied
                if (!string.IsNullOrWhiteSpace(NewPassword))
                {
                    if (string.IsNullOrWhiteSpace(CurrentPassword))
                    {
                        ErrorMessage = "Current password is required to change password.";
                        return;
                    }

                    if (!PasswordHasher.VerifyPassword(CurrentPassword, user.PasswordHash))
                    {
                        ErrorMessage = "Current password is incorrect.";
                        return;
                    }

                    if (NewPassword.Length < 6)
                    {
                        ErrorMessage = "New password must be at least 6 characters.";
                        return;
                    }

                    if (NewPassword != ConfirmNewPassword)
                    {
                        ErrorMessage = "New password and confirmation do not match.";
                        return;
                    }

                    user.PasswordHash = PasswordHasher.HashPassword(NewPassword);
                    CurrentPassword = string.Empty;
                    NewPassword = string.Empty;
                    ConfirmNewPassword = string.Empty;
                }

                _dbContext.Users.Update(user);
                await _dbContext.SaveChangesAsync();

                _sessionService.SetUser(user);
                SuccessMessage = "Profile updated successfully!";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to update profile: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
