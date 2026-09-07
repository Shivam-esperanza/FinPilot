using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinPilot.Interfaces;
using FinPilot.Models;

namespace FinPilot.ViewModels
{
    public partial class LoansViewModel : ObservableObject
    {
        private readonly ILoanTrackingService _loanService;
        private readonly IAccountService _accountService;
        private readonly IUserSessionService _sessionService;

        [ObservableProperty]
        private ObservableCollection<Loan> _loans = new();

        [ObservableProperty]
        private ObservableCollection<Bank> _banks = new();

        [ObservableProperty]
        private Bank? _selectedBank;

        [ObservableProperty]
        private string _loanNumber = string.Empty;

        [ObservableProperty]
        private string _loanType = "Personal"; // Personal, Home, Car, Education

        [ObservableProperty]
        private string _totalAmountText = string.Empty;

        [ObservableProperty]
        private string _remainingPrincipalText = string.Empty;

        [ObservableProperty]
        private string _monthlyEmiText = string.Empty;

        [ObservableProperty]
        private string _interestRateText = string.Empty;

        [ObservableProperty]
        private DateTime _nextEmiDate = DateTime.UtcNow.AddDays(10);

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public LoansViewModel(
            ILoanTrackingService loanService,
            IAccountService accountService,
            IUserSessionService sessionService)
        {
            _loanService = loanService;
            _accountService = accountService;
            _sessionService = sessionService;
        }

        [RelayCommand]
        public async Task LoadLoansAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                Guid userId = _sessionService.CurrentUserId ?? Guid.Empty;
                if (userId == Guid.Empty) return;

                var banksList = await _accountService.GetAvailableBanksAsync();
                Banks = new ObservableCollection<Bank>(banksList);

                var userLoans = await _loanService.GetUserLoansAsync(userId);
                Loans = new ObservableCollection<Loan>(userLoans);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading loans: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task AddLoanAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                Guid userId = _sessionService.CurrentUserId ?? Guid.Empty;
                if (userId == Guid.Empty) return;

                if (SelectedBank == null)
                {
                    ErrorMessage = "Please select a bank.";
                    return;
                }

                if (!decimal.TryParse(TotalAmountText, out decimal totalAmount) || totalAmount <= 0)
                {
                    ErrorMessage = "Please enter a valid positive loan amount.";
                    return;
                }

                if (!decimal.TryParse(RemainingPrincipalText, out decimal remaining) || remaining < 0)
                {
                    remaining = totalAmount;
                }

                if (!decimal.TryParse(MonthlyEmiText, out decimal emi) || emi <= 0)
                {
                    ErrorMessage = "Please enter a valid monthly EMI amount.";
                    return;
                }

                if (!decimal.TryParse(InterestRateText, out decimal rate) || rate < 0)
                {
                    rate = 10.5m;
                }

                var newLoan = new Loan
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    BankId = SelectedBank.Id,
                    LoanNumber = string.IsNullOrWhiteSpace(LoanNumber) ? $"LN-{DateTime.UtcNow.Ticks.ToString().Substring(10)}" : LoanNumber.Trim(),
                    LoanType = string.IsNullOrWhiteSpace(LoanType) ? "Personal" : LoanType.Trim(),
                    TotalLoanAmount = totalAmount,
                    RemainingPrincipal = remaining,
                    MonthlyEmi = emi,
                    InterestRate = rate,
                    NextEmiDate = NextEmiDate,
                    TotalTenureMonths = 36,
                    RemainingTenureMonths = 30,
                    LoanStartDate = DateTime.UtcNow
                };

                await _loanService.AddLoanAsync(newLoan);

                LoanNumber = string.Empty;
                TotalAmountText = string.Empty;
                RemainingPrincipalText = string.Empty;
                MonthlyEmiText = string.Empty;
                InterestRateText = string.Empty;

                IsBusy = false;
                await LoadLoansAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to add loan: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task DeleteLoanAsync(Loan loan)
        {
            if (loan == null) return;
            Guid userId = _sessionService.CurrentUserId ?? Guid.Empty;

            bool success = await _loanService.DeleteLoanAsync(loan.Id, userId);
            if (success)
            {
                Loans.Remove(loan);
            }
        }
    }
}
