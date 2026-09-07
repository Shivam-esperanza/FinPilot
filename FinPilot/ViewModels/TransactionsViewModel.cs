using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinPilot.Interfaces;
using FinPilot.Models;

namespace FinPilot.ViewModels
{
    public partial class TransactionsViewModel : ObservableObject
    {
        private readonly ITransactionService _txnService;
        private readonly IUserSessionService _sessionService;

        [ObservableProperty]
        private ObservableCollection<Transaction> _transactions = new();

        [ObservableProperty]
        private string _amountText = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private string _category = "Shopping"; // Shopping, LoanPayment, Fuel, Salary, Utilities

        [ObservableProperty]
        private DateTime _transactionDate = DateTime.UtcNow;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public TransactionsViewModel(ITransactionService txnService, IUserSessionService sessionService)
        {
            _txnService = txnService;
            _sessionService = sessionService;
        }

        [RelayCommand]
        public async Task LoadTransactionsAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                Guid userId = _sessionService.CurrentUserId ?? Guid.Empty;
                if (userId == Guid.Empty) return;

                var userTxns = await _txnService.GetUserTransactionsAsync(userId);
                Transactions = new ObservableCollection<Transaction>(userTxns);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading transactions: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task AddTransactionAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                Guid userId = _sessionService.CurrentUserId ?? Guid.Empty;
                if (userId == Guid.Empty) return;

                if (!decimal.TryParse(AmountText, out decimal amount) || amount <= 0)
                {
                    ErrorMessage = "Please enter a valid positive transaction amount.";
                    return;
                }

                if (string.IsNullOrWhiteSpace(Description))
                {
                    ErrorMessage = "Description is required.";
                    return;
                }

                var newTxn = new Transaction
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Amount = amount,
                    Description = Description.Trim(),
                    Category = string.IsNullOrWhiteSpace(Category) ? "General" : Category.Trim(),
                    TransactionDate = TransactionDate
                };

                await _txnService.AddTransactionAsync(newTxn);

                AmountText = string.Empty;
                Description = string.Empty;

                IsBusy = false;
                await LoadTransactionsAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to add transaction: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task DeleteTransactionAsync(Transaction transaction)
        {
            if (transaction == null) return;
            Guid userId = _sessionService.CurrentUserId ?? Guid.Empty;

            bool success = await _txnService.DeleteTransactionAsync(transaction.Id, userId);
            if (success)
            {
                Transactions.Remove(transaction);
            }
        }
    }
}
