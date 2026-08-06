using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FinPilot.Interfaces;
using FinPilot.Models;
using FinPilot.Database;

namespace FinPilot.Services
{
    public class LoanTrackingService : ILoanTrackingService
    {
        private readonly AppDbContext _dbContext;

        public LoanTrackingService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AutoUpdateBalancesAsync(Guid userId)
        {
            // 1. Gather all active liabilities linked to the profile
            var activeLoans = await _dbContext.Loans.Include(c=>c.Bank)
                .Where(l => l.UserId == userId && l.RemainingPrincipal > 0)
                .ToListAsync();

            if (!activeLoans.Any()) return;

            DateTime currentDate = DateTime.UtcNow;
            bool dataMutated = false;

            foreach (var loan in activeLoans)
            {
                // 2. Identify target milestones based on expected monthly boundaries
                // We use NextEmiDate minus 1 month as our baseline anchor point for calculation comparison
                DateTime baselineAnchor = loan.NextEmiDate.AddMonths(-1);

                int elapsedMonths = 0;

                // Track how many months have rolled past since our baseline caught up
                while (currentDate >= loan.NextEmiDate)
                {
                    elapsedMonths++;
                    loan.NextEmiDate = loan.NextEmiDate.AddMonths(1);
                }

                // 3. If months have rolled past while away, process the compounding decay matrix
                if (elapsedMonths > 0)
                {
                    dataMutated = true;

                    for (int i = 0; i < elapsedMonths; i++)
                    {
                        if (loan.RemainingPrincipal <= 0) break;

                        // Reducing-balance banking formula math:
                        // Monthly Interest Component = Current Outstanding Principal * (Annual Rate / 12 months / 100)
                        decimal monthlyInterestDrag = loan.RemainingPrincipal * (loan.InterestRate / 12m / 100m);

                        // Principal Component portion = Flat Monthly EMI paid - calculated interest penalty drag
                        decimal principalComponentPaid = loan.MonthlyEmi - monthlyInterestDrag;

                        if (principalComponentPaid > loan.RemainingPrincipal)
                        {
                            principalComponentPaid = loan.RemainingPrincipal;
                        }

                        // Decay outstanding parameters down the tree
                        loan.RemainingPrincipal -= principalComponentPaid;
                        loan.RemainingTenureMonths--;
                    }

                    // 4. Record historical financial transaction markers inside our database ledger
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

            // 5. Commit mutations safely to our physical SQLite file structure
            if (dataMutated)
            {
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
