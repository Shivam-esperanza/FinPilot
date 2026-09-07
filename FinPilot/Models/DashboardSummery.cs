using System;

namespace FinPilot.Models
{
    public class DashboardSummary
    {
        public string UserName { get; set; } = string.Empty;
        public int CreditScore { get; set; }
        public decimal TotalMonthlyIncome { get; set; }

        // Account Metrics
        public int AccountsCount { get; set; }
        public decimal TotalAccountBalance { get; set; }

        // Active Loan Metrics
        public int ActiveLoansCount { get; set; }
        public decimal TotalRemainingLoanAmount { get; set; }
        public decimal TotalUpcomingMonthlyEmi { get; set; }

        // Credit Card Metrics
        public int CreditCardsCount { get; set; }
        public decimal TotalCreditLimit { get; set; }
        public decimal TotalOutstandingAmount { get; set; }
        public double GlobalCreditUtilizationPercentage { get; set; }

        // Advanced Financial Health Analytics (Feature 7 & Phase 9)
        public int FinancialHealthScore { get; set; } // Computed on a scale of 0 to 100
        public string FinancialHealthStatus { get; set; } = "Good"; // Excellent, Good, Fair, Needs Attention
        public double MonthlyEmiToIncomeRatio { get; set; } // Percentage of income consumed by debt
        public int TotalUpcomingPaymentsCount { get; set; }
    }
}
