using System;
using System.Collections.Generic;
using System.Text;

namespace FinPilot.Models
{
    public class AdvisorOption
    {
        public string BankName { get; set; } = string.Empty; // e.g., "HDFC Bank", "ICICI Bank"
        public string FinancingType { get; set; } = string.Empty; // NoCostEmi, CreditCardEmi, PersonalLoan
        public int TenureMonths { get; set; }
        public decimal MonthlyEmi { get; set; }
        public decimal InterestRate { get; set; }
        public decimal TotalInterestPayable { get; set; }
        public decimal ProcessingFee { get; set; }
        public decimal InstantCashback { get; set; }

        // Final net financial footprint equation property
        public decimal NetEffectiveCost { get; set; }

        // Multi-Criteria Weighted Performance Score (0.0 to 100.0)
        public double MatchScore { get; set; }
        public string RecommendationReason { get; set; } = string.Empty; // Textual explanation for Feature 2
    }
}
