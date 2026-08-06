using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinPilot.Models;

namespace FinPilot.Interfaces
{
    public interface ICreditCardService
    {
        // Fetches all active credit card records along with their bank details for a specific user
        Task<List<CreditCard>> GetUserCardsAsync(Guid userId);

        // Simulates logging a purchase transaction on a card, adjusting available and outstanding limits
        Task<bool> LogTransactionAsync(Guid cardId, decimal amount, string description, string category);
    }
}
