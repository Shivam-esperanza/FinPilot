using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FinPilot.Database;
using FinPilot.Interfaces;
using FinPilot.Models;

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

        public async Task<CreditCard> AddCardAsync(CreditCard card)
        {
            if (card == null) throw new ArgumentNullException(nameof(card));
            if (card.Id == Guid.Empty) card.Id = Guid.NewGuid();

            if (card.AvailableLimit == 0 && card.CreditLimit > card.OutstandingAmount)
            {
                card.AvailableLimit = card.CreditLimit - card.OutstandingAmount;
            }

            await _dbContext.CreditCards.AddAsync(card);
            await _dbContext.SaveChangesAsync();
            return card;
        }

        public async Task<CreditCard> UpdateCardAsync(CreditCard card)
        {
            if (card == null) throw new ArgumentNullException(nameof(card));

            var existing = await _dbContext.CreditCards.FirstOrDefaultAsync(c => c.Id == card.Id && c.UserId == card.UserId);
            if (existing == null) throw new InvalidOperationException("Credit card not found.");

            existing.CardName = card.CardName;
            existing.LastFourDigits = card.LastFourDigits;
            existing.CreditLimit = card.CreditLimit;
            existing.OutstandingAmount = card.OutstandingAmount;
            existing.AvailableLimit = Math.Max(0, card.CreditLimit - card.OutstandingAmount);
            existing.DueDate = card.DueDate;
            existing.StatementDate = card.StatementDate;
            existing.MinimumPayment = card.MinimumPayment;
            existing.BankId = card.BankId;
            existing.LastModifiedAt = DateTime.UtcNow;

            _dbContext.CreditCards.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteCardAsync(Guid cardId, Guid userId)
        {
            var card = await _dbContext.CreditCards.FirstOrDefaultAsync(c => c.Id == cardId && c.UserId == userId);
            if (card == null) return false;

            _dbContext.CreditCards.Remove(card);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> LogTransactionAsync(Guid cardId, decimal amount, string description, string category)
        {
            var card = await _dbContext.CreditCards.FirstOrDefaultAsync(c => c.Id == cardId);
            if (card == null || card.AvailableLimit < amount) return false;

            card.AvailableLimit -= amount;
            card.OutstandingAmount += amount;

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
