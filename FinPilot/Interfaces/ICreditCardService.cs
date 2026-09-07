using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinPilot.Models;

namespace FinPilot.Interfaces
{
    public interface ICreditCardService
    {
        Task<List<CreditCard>> GetUserCardsAsync(Guid userId);
        Task<CreditCard> AddCardAsync(CreditCard card);
        Task<CreditCard> UpdateCardAsync(CreditCard card);
        Task<bool> DeleteCardAsync(Guid cardId, Guid userId);
        Task<bool> LogTransactionAsync(Guid cardId, decimal amount, string description, string category);
    }
}
