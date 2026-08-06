using System;
using System.Collections.Generic;
using System.Text;


namespace FinPilot.Models
{
    public class Loan
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid BankId { get; set; }
        public string LoanNumber { get; set; } = string.Empty;
        public string LoanType { get; set; } = string.Empty; // e.g., Home, Car, Personal
        public decimal TotalLoanAmount { get; set; }
        public decimal RemainingPrincipal { get; set; }
        public decimal InterestRate { get; set; } // Annual Percentage Rate (APR)
        public int TotalTenureMonths { get; set; }
        public int RemainingTenureMonths { get; set; }
        public decimal MonthlyEmi { get; set; }
        public DateTime NextEmiDate { get; set; }
        public DateTime LoanStartDate { get; set; }
        public int MissedPaymentsCount { get; set; }
        public DateTime PredictedClosureDate { get; set; }
        public bool IsEligibleForRefinancing { get; set; } // Powers Feature 8's refinancing alert
        public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;
        public bool IsSynced { get; set; } = false;


        // Navigation Properties for Entity Framework Core
        public User? User { get; set; }
        public Bank? Bank { get; set; }
    }
}

