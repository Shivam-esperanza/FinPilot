using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FinPilot.Interfaces;
using FinPilot.Models;
using FinPilot.Database;
using FinPilot.Services;
using FinPilot.ViewModels;

namespace FinPilot.Services
{
    public class CreditCardService : ICreditCardService
    {
        private readonly AppDbContext _dbContext;

        public CreditCardService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<CreditCard>> GetUserCardsAsync(Guid userId)
        {
            return await _dbContext.CreditCards
                .Include(c => c.Bank)
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> LogTransactionAsync(Guid cardId, decimal amount, string description, string category)
        {
            var card = await _dbContext.CreditCards.FirstOrDefaultAsync(c => c.Id == cardId);
            if (card == null || card.AvailableLimit < amount) return false;

            // 1. Adjust credit line distributions
            card.AvailableLimit -= amount;
            card.OutstandingAmount += amount;

            // 2. Write an audit trail transaction marker to our ledger
            var txn = new Transaction
            {
                Id = Guid.NewGuid(),
                UserId = card.UserId,
                CreditCardId = card.Id,
                Amount = amount,
                TransactionDate = DateTime.UtcNow,
                Description = description,
                Category = category
            };

            await _dbContext.Transactions.AddAsync(txn);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
