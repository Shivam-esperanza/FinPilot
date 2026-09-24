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

        private static readonly string[] IncomeCategories = new[] { "Salary", "Income", "Refund", "Deposit", "Credit" };

        public TransactionService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Transaction>> GetUserTransactionsAsync(Guid userId)
        {
            if (userId == Guid.Empty) return new List<Transaction>();

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

            string category = transaction.Category ?? string.Empty;
            bool isIncome = IncomeCategories.Any(c => c.Equals(category, StringComparison.OrdinalIgnoreCase));

            // 1. Update Credit Card if linked
            if (transaction.CreditCardId.HasValue && transaction.CreditCardId.Value != Guid.Empty)
            {
                var card = await _dbContext.CreditCards.FirstOrDefaultAsync(c => c.Id == transaction.CreditCardId.Value);
                if (card != null)
                {
                    if (isIncome)
                    {
                        card.OutstandingAmount = Math.Max(0, card.OutstandingAmount - transaction.Amount);
                        card.AvailableLimit = Math.Min(card.CreditLimit, card.AvailableLimit + transaction.Amount);
                    }
                    else
                    {
                        card.OutstandingAmount += transaction.Amount;
                        card.AvailableLimit = Math.Max(0, card.AvailableLimit - transaction.Amount);
                    }
                    _dbContext.CreditCards.Update(card);
                }
            }

            // 2. Update Loan if linked
            if (transaction.LoanId.HasValue && transaction.LoanId.Value != Guid.Empty)
            {
                var loan = await _dbContext.Loans.FirstOrDefaultAsync(l => l.Id == transaction.LoanId.Value);
                if (loan != null)
                {
                    loan.RemainingPrincipal = Math.Max(0, loan.RemainingPrincipal - transaction.Amount);
                    _dbContext.Loans.Update(loan);
                }
            }

            // 3. Update Bank Account Balance
            var userAccount = await _dbContext.Accounts
                .Where(a => a.UserId == transaction.UserId)
                .OrderByDescending(a => a.IsPrimaryRelationship)
                .FirstOrDefaultAsync();

            if (userAccount != null)
            {
                if (isIncome)
                {
                    userAccount.Balance += transaction.Amount;
                }
                else
                {
                    userAccount.Balance -= transaction.Amount;
                }
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

            string existingCategory = existing.Category ?? string.Empty;
            bool wasIncome = IncomeCategories.Any(c => c.Equals(existingCategory, StringComparison.OrdinalIgnoreCase));
            var userAccount = await _dbContext.Accounts
                .Where(a => a.UserId == transaction.UserId)
                .OrderByDescending(a => a.IsPrimaryRelationship)
                .FirstOrDefaultAsync();

            if (userAccount != null)
            {
                if (wasIncome) userAccount.Balance -= existing.Amount;
                else userAccount.Balance += existing.Amount;
            }

            string newCategory = transaction.Category ?? string.Empty;
            bool isIncome = IncomeCategories.Any(c => c.Equals(newCategory, StringComparison.OrdinalIgnoreCase));
            if (userAccount != null)
            {
                if (isIncome) userAccount.Balance += transaction.Amount;
                else userAccount.Balance -= transaction.Amount;
                _dbContext.Accounts.Update(userAccount);
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
            if (transactionId == Guid.Empty || userId == Guid.Empty) return false;

            var txn = await _dbContext.Transactions.FirstOrDefaultAsync(t => t.Id == transactionId && t.UserId == userId);
            if (txn == null) return false;

            string category = txn.Category ?? string.Empty;
            bool isIncome = IncomeCategories.Any(c => c.Equals(category, StringComparison.OrdinalIgnoreCase));

            // Restore/reverse bank account balance
            var userAccount = await _dbContext.Accounts
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsPrimaryRelationship)
                .FirstOrDefaultAsync();

            if (userAccount != null)
            {
                if (isIncome) userAccount.Balance -= txn.Amount;
                else userAccount.Balance += txn.Amount;
                _dbContext.Accounts.Update(userAccount);
            }

            // Reverse Credit Card if linked
            if (txn.CreditCardId.HasValue && txn.CreditCardId.Value != Guid.Empty)
            {
                var card = await _dbContext.CreditCards.FirstOrDefaultAsync(c => c.Id == txn.CreditCardId.Value);
                if (card != null)
                {
                    if (isIncome)
                    {
                        card.OutstandingAmount += txn.Amount;
                        card.AvailableLimit = Math.Max(0, card.AvailableLimit - txn.Amount);
                    }
                    else
                    {
                        card.OutstandingAmount = Math.Max(0, card.OutstandingAmount - txn.Amount);
                        card.AvailableLimit = Math.Min(card.CreditLimit, card.AvailableLimit + txn.Amount);
                    }
                    _dbContext.CreditCards.Update(card);
                }
            }

            _dbContext.Transactions.Remove(txn);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
