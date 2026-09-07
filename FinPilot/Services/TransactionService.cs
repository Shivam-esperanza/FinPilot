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
    public class TransactionService : ITransactionService
    {
        private readonly AppDbContext _dbContext;

        public TransactionService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Transaction>> GetUserTransactionsAsync(Guid userId)
        {
            return await _dbContext.Transactions
                .Include(t => t.Loan)
                .Include(t => t.CreditCard)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<Transaction> AddTransactionAsync(Transaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            if (transaction.Id == Guid.Empty) transaction.Id = Guid.NewGuid();

            // Deduct transaction amount from user's primary or active bank account
            var userAccount = await _dbContext.Accounts
                .Where(a => a.UserId == transaction.UserId)
                .OrderByDescending(a => a.IsPrimaryRelationship)
                .FirstOrDefaultAsync();

            if (userAccount != null)
            {
                userAccount.Balance -= transaction.Amount;
                _dbContext.Accounts.Update(userAccount);
            }

            await _dbContext.Transactions.AddAsync(transaction);
            await _dbContext.SaveChangesAsync();
            return transaction;
        }

        public async Task<Transaction> UpdateTransactionAsync(Transaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));

            var existing = await _dbContext.Transactions.FirstOrDefaultAsync(t => t.Id == transaction.Id && t.UserId == transaction.UserId);
            if (existing == null) throw new InvalidOperationException("Transaction not found.");

            decimal amountDiff = transaction.Amount - existing.Amount;
            if (amountDiff != 0)
            {
                var userAccount = await _dbContext.Accounts
                    .Where(a => a.UserId == transaction.UserId)
                    .OrderByDescending(a => a.IsPrimaryRelationship)
                    .FirstOrDefaultAsync();

                if (userAccount != null)
                {
                    userAccount.Balance -= amountDiff;
                    _dbContext.Accounts.Update(userAccount);
                }
            }

            existing.Amount = transaction.Amount;
            existing.Description = transaction.Description;
            existing.Category = transaction.Category;
            existing.TransactionDate = transaction.TransactionDate;
            existing.LoanId = transaction.LoanId;
            existing.CreditCardId = transaction.CreditCardId;

            _dbContext.Transactions.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteTransactionAsync(Guid transactionId, Guid userId)
        {
            var txn = await _dbContext.Transactions.FirstOrDefaultAsync(t => t.Id == transactionId && t.UserId == userId);
            if (txn == null) return false;

            // Restore deducted amount back to user's bank account
            var userAccount = await _dbContext.Accounts
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsPrimaryRelationship)
                .FirstOrDefaultAsync();

            if (userAccount != null)
            {
                userAccount.Balance += txn.Amount;
                _dbContext.Accounts.Update(userAccount);
            }

            _dbContext.Transactions.Remove(txn);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
