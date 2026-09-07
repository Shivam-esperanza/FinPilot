using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinPilot.Interfaces;
using FinPilot.Models;

namespace FinPilot.ViewModels
{
    public partial class CreditCardManagerViewModel : ObservableObject
    {
        private readonly ICreditCardService _cardService;
        private readonly IAccountService _accountService;
        private readonly IUserSessionService _sessionService;

        [ObservableProperty]
        private ObservableCollection<CreditCard> _cards = new();

        [ObservableProperty]
        private ObservableCollection<Bank> _banks = new();

        [ObservableProperty]
        private Bank? _selectedBank;

        [ObservableProperty]
        private string _cardName = string.Empty;

        [ObservableProperty]
        private string _lastFourDigits = string.Empty;

        [ObservableProperty]
        private string _creditLimitText = string.Empty;

        [ObservableProperty]
        private string _outstandingAmountText = string.Empty;

        [ObservableProperty]
        private DateTime _dueDate = DateTime.UtcNow.AddDays(15);

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public CreditCardManagerViewModel(
            ICreditCardService cardService,
            IAccountService accountService,
            IUserSessionService sessionService)
        {
            _cardService = cardService;
            _accountService = accountService;
            _sessionService = sessionService;
        }

        [RelayCommand]
        public async Task LoadCardsAsync()
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

                var userCards = await _cardService.GetUserCardsAsync(userId);
                Cards = new ObservableCollection<CreditCard>(userCards);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading credit cards: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task AddCardAsync()
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

                if (string.IsNullOrWhiteSpace(CardName))
                {
                    ErrorMessage = "Card name is required.";
                    return;
                }

                if (!decimal.TryParse(CreditLimitText, out decimal limit) || limit <= 0)
                {
                    ErrorMessage = "Please enter a valid credit limit.";
                    return;
                }

                if (!decimal.TryParse(OutstandingAmountText, out decimal outstanding) || outstanding < 0)
                {
                    outstanding = 0m;
                }

                var newCard = new CreditCard
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    BankId = SelectedBank.Id,
                    CardName = CardName.Trim(),
                    LastFourDigits = string.IsNullOrWhiteSpace(LastFourDigits) ? "1234" : LastFourDigits.Trim(),
                    CreditLimit = limit,
                    OutstandingAmount = outstanding,
                    AvailableLimit = Math.Max(0, limit - outstanding),
                    DueDate = DueDate,
                    StatementDate = DueDate.AddDays(-20)
                };

                await _cardService.AddCardAsync(newCard);

                CardName = string.Empty;
                LastFourDigits = string.Empty;
                CreditLimitText = string.Empty;
                OutstandingAmountText = string.Empty;

                IsBusy = false;
                await LoadCardsAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to add credit card: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task DeleteCardAsync(CreditCard card)
        {
            if (card == null) return;
            Guid userId = _sessionService.CurrentUserId ?? Guid.Empty;

            bool success = await _cardService.DeleteCardAsync(card.Id, userId);
            if (success)
            {
                Cards.Remove(card);
            }
        }
    }
}
