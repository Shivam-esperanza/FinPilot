using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinPilot.Models;

namespace FinPilot.Interfaces
{
    public interface IAccountService
    {
        Task<List<Account>> GetUserAccountsAsync(Guid userId);
        Task<List<Bank>> GetAvailableBanksAsync();
        Task<Account> AddAccountAsync(Account account);
        Task<Account> UpdateAccountAsync(Account account);
        Task<bool> DeleteAccountAsync(Guid accountId, Guid userId);
    }
}
