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
    public class LoanTrackingService : ILoanTrackingService
    {
        private readonly AppDbContext _dbContext;

        public LoanTrackingService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Loan>> GetUserLoansAsync(Guid userId)
        {
            return await _dbContext.Loans
                .Include(l => l.Bank)
                .Where(l => l.UserId == userId)
                .ToListAsync();
        }

        public async Task<Loan> AddLoanAsync(Loan loan)
        {
            if (loan == null) throw new ArgumentNullException(nameof(loan));
            if (loan.Id == Guid.Empty) loan.Id = Guid.NewGuid();

            if (loan.TotalLoanAmount < 0 || loan.RemainingPrincipal < 0 || loan.MonthlyEmi < 0)
            {
                throw new ArgumentException("Loan financial amounts cannot be negative.");
            }

            await _dbContext.Loans.AddAsync(loan);
            await _dbContext.SaveChangesAsync();
            return loan;
        }

        public async Task<Loan> UpdateLoanAsync(Loan loan)
        {
            if (loan == null) throw new ArgumentNullException(nameof(loan));

            var existing = await _dbContext.Loans.FirstOrDefaultAsync(l => l.Id == loan.Id && l.UserId == loan.UserId);
            if (existing == null) throw new InvalidOperationException("Loan not found.");

            existing.LoanNumber = loan.LoanNumber;
            existing.LoanType = loan.LoanType;
            existing.TotalLoanAmount = loan.TotalLoanAmount;
            existing.RemainingPrincipal = loan.RemainingPrincipal;
            existing.InterestRate = loan.InterestRate;
            existing.TotalTenureMonths = loan.TotalTenureMonths;
            existing.RemainingTenureMonths = loan.RemainingTenureMonths;
            existing.MonthlyEmi = loan.MonthlyEmi;
            existing.NextEmiDate = loan.NextEmiDate;
            existing.BankId = loan.BankId;
            existing.LastModifiedAt = DateTime.UtcNow;

            _dbContext.Loans.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteLoanAsync(Guid loanId, Guid userId)
        {
            var loan = await _dbContext.Loans.FirstOrDefaultAsync(l => l.Id == loanId && l.UserId == userId);
            if (loan == null) return false;

            _dbContext.Loans.Remove(loan);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task AutoUpdateBalancesAsync(Guid userId)
        {
            var activeLoans = await _dbContext.Loans
                .Include(c => c.Bank)
                .Where(l => l.UserId == userId && l.RemainingPrincipal > 0)
                .ToListAsync();

            if (!activeLoans.Any()) return;

            DateTime currentDate = DateTime.UtcNow;
            bool dataMutated = false;

            foreach (var loan in activeLoans)
            {
                int elapsedMonths = 0;
                while (currentDate >= loan.NextEmiDate)
                {
                    elapsedMonths++;
                    loan.NextEmiDate = loan.NextEmiDate.AddMonths(1);
                }

                if (elapsedMonths > 0)
                {
                    dataMutated = true;
                    for (int i = 0; i < elapsedMonths; i++)
                    {
                        if (loan.RemainingPrincipal <= 0) break;

                        decimal monthlyInterestDrag = loan.RemainingPrincipal * (loan.InterestRate / 12m / 100m);
                        decimal principalComponentPaid = loan.MonthlyEmi - monthlyInterestDrag;

                        if (principalComponentPaid > loan.RemainingPrincipal)
                        {
                            principalComponentPaid = loan.RemainingPrincipal;
                        }

                        loan.RemainingPrincipal -= principalComponentPaid;
                        loan.RemainingTenureMonths--;
                    }

                    var autoTransaction = new Transaction
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        LoanId = loan.Id,
                        Amount = loan.MonthlyEmi * elapsedMonths,
                        TransactionDate = currentDate,
                        Description = $"Auto-Decay Engine Synchronized: Processed {elapsedMonths} pending monthly payments.",
                        Category = "LoanPayment"
                    };

                    await _dbContext.Transactions.AddAsync(autoTransaction);
                }
            }

            if (dataMutated)
            {
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
