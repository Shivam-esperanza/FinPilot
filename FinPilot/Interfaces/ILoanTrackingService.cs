using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinPilot.Models;

namespace FinPilot.Interfaces
{
    public interface ILoanTrackingService
    {
        Task<List<Loan>> GetUserLoansAsync(Guid userId);
        Task<Loan> AddLoanAsync(Loan loan);
        Task<Loan> UpdateLoanAsync(Loan loan);
        Task<bool> DeleteLoanAsync(Guid loanId, Guid userId);
        Task AutoUpdateBalancesAsync(Guid userId);
    }
}
