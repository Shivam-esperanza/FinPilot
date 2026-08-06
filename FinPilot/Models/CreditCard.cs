using System;
using System.Collections.Generic;
using System.Text;

namespace FinPilot.Models
{
    public class CreditCard
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid BankId { get; set; }
        public string CardName { get; set; } = string.Empty; // e.g., "Regalia", "Amazon Pay"
        public string LastFourDigits { get; set; } = string.Empty;
        public decimal CreditLimit { get; set; }
        public decimal AvailableLimit { get; set; }
        public decimal OutstandingAmount { get; set; }
        public DateTime StatementDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal RewardPointsBalance { get; set; }
        public decimal TotalCashbackEarned { get; set; }
        public decimal MinimumPayment { get; set; }
        public decimal AnnualFee { get; set; }
        public DateTime AnnualFeeDueDate { get; set; }
        public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;
        public bool IsSynced { get; set; } = false;

        // Computed Property (Calculates utilization on the fly for Feature 4 & 8)
        public double CreditUtilizationPercentage =>
            CreditLimit > 0 ? (double)(OutstandingAmount / CreditLimit) * 100 : 0;


        // Navigation Properties for Entity Framework Core
        public User? User { get; set; }
        public Bank? Bank { get; set; }
    }
}

