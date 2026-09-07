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
    public class AccountService : IAccountService
    {
        private readonly AppDbContext _dbContext;

        public AccountService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Account>> GetUserAccountsAsync(Guid userId)
        {
            return await _dbContext.Accounts
                .Include(a => a.Bank)
                .Where(a => a.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<Bank>> GetAvailableBanksAsync()
        {
            var existingBanks = await _dbContext.Banks.ToListAsync();

            var defaultBanks = new List<Bank>
            {
                new Bank { Id = Guid.NewGuid(), Name = "ICICI Bank", Code = "ICICI01", IsPartnerBank = true },
                new Bank { Id = Guid.NewGuid(), Name = "HDFC Bank", Code = "HDFC01", IsPartnerBank = true },
                new Bank { Id = Guid.NewGuid(), Name = "State Bank of India (SBI)", Code = "SBI01", IsPartnerBank = true },
                new Bank { Id = Guid.NewGuid(), Name = "Axis Bank", Code = "AXIS01", IsPartnerBank = true },
                new Bank { Id = Guid.NewGuid(), Name = "Kotak Mahindra Bank", Code = "KOTAK01", IsPartnerBank = true },
                new Bank { Id = Guid.NewGuid(), Name = "IDFC FIRST Bank", Code = "IDFC01", IsPartnerBank = true },
                new Bank { Id = Guid.NewGuid(), Name = "Bank of Baroda", Code = "BOB01", IsPartnerBank = false },
                new Bank { Id = Guid.NewGuid(), Name = "Punjab National Bank", Code = "PNB01", IsPartnerBank = false },
                new Bank { Id = Guid.NewGuid(), Name = "IndusInd Bank", Code = "INDUS01", IsPartnerBank = false },
                new Bank { Id = Guid.NewGuid(), Name = "Canara Bank", Code = "CANARA01", IsPartnerBank = false },
                new Bank { Id = Guid.NewGuid(), Name = "Yes Bank", Code = "YES01", IsPartnerBank = false },
                new Bank { Id = Guid.NewGuid(), Name = "Federal Bank", Code = "FED01", IsPartnerBank = false },
                new Bank { Id = Guid.NewGuid(), Name = "Union Bank of India", Code = "UNION01", IsPartnerBank = false },
                new Bank { Id = Guid.NewGuid(), Name = "Standard Chartered", Code = "SCB01", IsPartnerBank = false },
                new Bank { Id = Guid.NewGuid(), Name = "HSBC Bank", Code = "HSBC01", IsPartnerBank = false }
            };

            bool addedNew = false;
            foreach (var b in defaultBanks)
            {
                if (!existingBanks.Any(x => x.Code == b.Code || x.Name == b.Name))
                {
                    await _dbContext.Banks.AddAsync(b);
                    existingBanks.Add(b);
                    addedNew = true;
                }
            }

            if (addedNew)
            {
                await _dbContext.SaveChangesAsync();
            }

            return existingBanks.OrderBy(b => b.Name).ToList();
        }

        public async Task<Account> AddAccountAsync(Account account)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));
            if (account.Id == Guid.Empty) account.Id = Guid.NewGuid();

            await _dbContext.Accounts.AddAsync(account);
            await _dbContext.SaveChangesAsync();
            return account;
        }

        public async Task<Account> UpdateAccountAsync(Account account)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));

            var existing = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == account.Id && a.UserId == account.UserId);
            if (existing == null) throw new InvalidOperationException("Account not found.");

            existing.AccountNumber = account.AccountNumber;
            existing.AccountType = account.AccountType;
            existing.Balance = account.Balance;
            existing.IsPrimaryRelationship = account.IsPrimaryRelationship;
            existing.BankId = account.BankId;

            _dbContext.Accounts.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAccountAsync(Guid accountId, Guid userId)
        {
            var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);
            if (account == null) return false;

            _dbContext.Accounts.Remove(account);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
