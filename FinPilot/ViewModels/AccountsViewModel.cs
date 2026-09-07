using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinPilot.Interfaces;
using FinPilot.Models;

namespace FinPilot.ViewModels
{
    public partial class AccountsViewModel : ObservableObject
    {
        private readonly IAccountService _accountService;
        private readonly IUserSessionService _sessionService;

        [ObservableProperty]
        private ObservableCollection<Account> _accounts = new();

        [ObservableProperty]
        private ObservableCollection<Bank> _banks = new();

        [ObservableProperty]
        private Bank? _selectedBank;

        [ObservableProperty]
        private string _accountNumber = string.Empty;

        [ObservableProperty]
        private string _accountType = "Savings"; // Savings, Salary, Current

        [ObservableProperty]
        private string _balanceText = string.Empty;

        [ObservableProperty]
        private bool _isPrimaryRelationship;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public AccountsViewModel(IAccountService accountService, IUserSessionService sessionService)
        {
            _accountService = accountService;
            _sessionService = sessionService;
        }

        [RelayCommand]
        public async Task LoadAccountsAsync()
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

                var userAccounts = await _accountService.GetUserAccountsAsync(userId);
                Accounts = new ObservableCollection<Account>(userAccounts);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading accounts: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task AddAccountAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                Guid userId = _sessionService.CurrentUserId ?? Guid.Empty;
                if (userId == Guid.Empty)
                {
                    ErrorMessage = "User session invalid.";
                    return;
                }

                if (SelectedBank == null)
                {
                    ErrorMessage = "Please select a bank.";
                    return;
                }

                if (string.IsNullOrWhiteSpace(AccountNumber))
                {
                    ErrorMessage = "Account number is required.";
                    return;
                }

                if (!decimal.TryParse(BalanceText, out decimal balance) || balance < 0)
                {
                    ErrorMessage = "Please enter a valid balance.";
                    return;
                }

                var newAcc = new Account
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    BankId = SelectedBank.Id,
                    AccountNumber = AccountNumber.Trim(),
                    AccountType = string.IsNullOrWhiteSpace(AccountType) ? "Savings" : AccountType.Trim(),
                    Balance = balance,
                    IsPrimaryRelationship = IsPrimaryRelationship
                };

                await _accountService.AddAccountAsync(newAcc);
                AccountNumber = string.Empty;
                BalanceText = string.Empty;

                IsBusy = false;
                await LoadAccountsAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to add account: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task DeleteAccountAsync(Account account)
        {
            if (account == null) return;
            Guid userId = _sessionService.CurrentUserId ?? Guid.Empty;

            bool success = await _accountService.DeleteAccountAsync(account.Id, userId);
            if (success)
            {
                Accounts.Remove(account);
            }
        }
    }
}
