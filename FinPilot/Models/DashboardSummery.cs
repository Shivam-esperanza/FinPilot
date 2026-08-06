using System;
using System.Collections.Generic;
using System.Text;

namespace FinPilot.Models
{
    public class DashboardSummary
    {
        public int CreditScore { get; set; }
        public decimal TotalMonthlyIncome { get; set; }

        // Active Loan Metrics
        public int ActiveLoansCount { get; set; }
        public decimal TotalRemainingLoanAmount { get; set; }
        public decimal TotalUpcomingMonthlyEmi { get; set; }

        // Credit Card Metrics
        public decimal TotalCreditLimit { get; set; }
        public decimal TotalOutstandingAmount { get; set; }
        public double GlobalCreditUtilizationPercentage { get; set; }

        // Advanced Financial Health Analytics (Feature 7)
        public int FinancialHealthScore { get; set; } // Computed on a scale of 0 to 100
        public double MonthlyEmiToIncomeRatio { get; set; } // Percentage of income consumed by debt
        public int TotalUpcomingPaymentsCount { get; set; }
    }
}
