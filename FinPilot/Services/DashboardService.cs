using System;
using System.Collections.Generic;
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
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new InvalidOperationException("User profile not found in local database context.");
            }

            var userAccounts = await _dbContext.Accounts.Where(a => a.UserId == userId).ToListAsync();
            var userLoans = await _dbContext.Loans.Where(l => l.UserId == userId).ToListAsync();
            var userCards = await _dbContext.CreditCards.Include(c => c.Bank).Where(c => c.UserId == userId).ToListAsync();

            decimal totalAccountBalance = userAccounts.Sum(a => a.Balance);
            decimal totalRemainingLoan = userLoans.Sum(l => l.RemainingPrincipal);
            decimal totalMonthlyEmi = userLoans.Sum(l => l.MonthlyEmi);

            decimal totalLimit = userCards.Sum(c => c.CreditLimit);
            decimal totalOutstanding = userCards.Sum(c => c.OutstandingAmount);

            double globalUtilization = totalLimit > 0 ? (double)(totalOutstanding / totalLimit) * 100 : 0;
            double emiToIncomeRatio = user.MonthlyIncome > 0 ? (double)(totalMonthlyEmi / user.MonthlyIncome) * 100 : 0;

            int upcomingPayments = userLoans.Count(l => l.NextEmiDate >= DateTime.UtcNow) +
                                   userCards.Count(c => c.DueDate >= DateTime.UtcNow);

            int calculatedHealthScore = CalculateFinancialHealthScore(user.CreditScore, globalUtilization, emiToIncomeRatio);
            string healthStatus = GetHealthStatusText(calculatedHealthScore);

            return new DashboardSummary
            {
                UserName = user.FullName,
                CreditScore = user.CreditScore,
                TotalMonthlyIncome = user.MonthlyIncome,
                AccountsCount = userAccounts.Count,
                TotalAccountBalance = totalAccountBalance,
                ActiveLoansCount = userLoans.Count,
                TotalRemainingLoanAmount = totalRemainingLoan,
                TotalUpcomingMonthlyEmi = totalMonthlyEmi,
                CreditCardsCount = userCards.Count,
                TotalCreditLimit = totalLimit,
                TotalOutstandingAmount = totalOutstanding,
                GlobalCreditUtilizationPercentage = Math.Round(globalUtilization, 2),
                MonthlyEmiToIncomeRatio = Math.Round(emiToIncomeRatio, 2),
                TotalUpcomingPaymentsCount = upcomingPayments,
                FinancialHealthScore = calculatedHealthScore,
                FinancialHealthStatus = healthStatus
            };
        }

        private int CalculateFinancialHealthScore(int creditScore, double creditUtilization, double emiRatio)
        {
            double finalScore = 0;

            // Metric A: Credit Score Contributions (40%)
            if (creditScore >= 750) finalScore += 40;
            else if (creditScore >= 700) finalScore += 30;
            else if (creditScore >= 650) finalScore += 20;
            else finalScore += 10;

            // Metric B: Credit Utilization Ratios (30%)
            if (creditUtilization <= 30) finalScore += 30;
            else if (creditUtilization <= 50) finalScore += 15;
            else finalScore += 5;

            // Metric C: EMI-to-Income Burden Ratios (30%)
            if (emiRatio <= 40) finalScore += 30;
            else if (emiRatio <= 60) finalScore += 15;
            else finalScore += 0;

            return (int)Math.Clamp(finalScore, 0, 100);
        }

        private string GetHealthStatusText(int score)
        {
            if (score >= 80) return "Excellent";
            if (score >= 65) return "Good";
            if (score >= 50) return "Fair";
            return "Needs Attention";
        }
    }
}
