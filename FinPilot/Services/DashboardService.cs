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
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _dbContext;

        public DashboardService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DashboardSummary> GetDashboardSummaryAsync(Guid userId)
        {
            // 1. Fetch user core record safely from local storage
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new InvalidOperationException("User profile not found in local context.");
            }

            // 2. Fetch tracking parameters asynchronously
            var userLoans = await _dbContext.Loans.Where(l => l.UserId == userId).ToListAsync();
            var userCards = await _dbContext.CreditCards.Include(c=>c.Bank).Where(c => c.UserId == userId).ToListAsync();

            // 3. Run structural aggregations
            decimal totalRemainingLoan = userLoans.Sum(l => l.RemainingPrincipal);
            decimal totalMonthlyEmi = userLoans.Sum(l => l.MonthlyEmi);

            decimal totalLimit = userCards.Sum(c => c.CreditLimit);
            decimal totalOutstanding = userCards.Sum(c => c.OutstandingAmount);

            // 4. Compute Financial Health Indicators (Feature 4 & Feature 7 Math)
            double globalUtilization = totalLimit > 0 ? (double)(totalOutstanding / totalLimit) * 100 : 0;
            double emiToIncomeRatio = user.MonthlyIncome > 0 ? (double)(totalMonthlyEmi / user.MonthlyIncome) * 100 : 0;

            int upcomingPayments = userLoans.Count(l => l.NextEmiDate >= DateTime.UtcNow) +
                                   userCards.Count(c => c.DueDate >= DateTime.UtcNow);

            int calculatedHealthScore = CalculateFinancialHealthScore(user.CreditScore, globalUtilization, emiToIncomeRatio);

            // 5. Package into data payload blueprint
            return new DashboardSummary
            {
                CreditScore = user.CreditScore,
                TotalMonthlyIncome = user.MonthlyIncome,
                ActiveLoansCount = userLoans.Count,
                TotalRemainingLoanAmount = totalRemainingLoan,
                TotalUpcomingMonthlyEmi = totalMonthlyEmi,
                TotalCreditLimit = totalLimit,
                TotalOutstandingAmount = totalOutstanding,
                GlobalCreditUtilizationPercentage = Math.Round(globalUtilization, 2),
                MonthlyEmiToIncomeRatio = Math.Round(emiToIncomeRatio, 2),
                TotalUpcomingPaymentsCount = upcomingPayments,
                FinancialHealthScore = calculatedHealthScore
            };
        }

        // Advanced Multi-Criteria Credit-Scoring Matrix Algorithm (Feature 7 Engine Core)
        private int CalculateFinancialHealthScore(int creditScore, double creditUtilization, double emiRatio)
        {
            // Baseline starting parameters out of a 100 max score
            double finalScore = 0;

            // Metric A: Credit Score Contributions (Weighted at 40% of overall score)
            // Excellent: 750+, Good: 700-749, Fair: 650-699, Poor: Below 650
            if (creditScore >= 750) finalScore += 40;
            else if (creditScore >= 700) finalScore += 30;
            else if (creditScore >= 650) finalScore += 20;
            else finalScore += 10;

            // Metric B: Credit Utilization Penetration Ratios (Weighted at 30% of overall score)
            // Optimal: Under 30%, Alert Zone: 30%-50%, Danger Zone: 50%+ (Feature 8 Alert logic matching)
            if (creditUtilization <= 30) finalScore += 30;
            else if (creditUtilization <= 50) finalScore += 15;
            else finalScore += 5;

            // Metric C: Debt-to-Income / EMI Burden Ratios (Weighted at 30% of overall score)
            // Healthy: Under 40% of income, Strained: 40%-60%, Overleveraged: 60%+
            if (emiRatio <= 40) finalScore += 30;
            else if (emiRatio <= 60) finalScore += 15;
            else finalScore += 0;

            return (int)Math.Clamp(finalScore, 0, 100);
        }
    }
}

