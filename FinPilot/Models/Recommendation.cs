using System;
using System.Collections.Generic;
using System;

namespace FinPilot.Models
{
    public class Recommendation
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid BankId { get; set; }
        public decimal ProductPrice { get; set; }
        public string ProductCategory { get; set; } = string.Empty; // e.g., Laptop, Smartphone
        public decimal EstimatedEmi { get; set; }
        public decimal TotalInterestPayable { get; set; }
        public decimal ExpectedCashback { get; set; }
        public double MatchScore { get; set; } // Percentage match, e.g., 95.0
        public string Status { get; set; } = string.Empty; // Pending, Accepted, Rejected
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public User? User { get; set; }
        public Bank? Bank { get; set; }
    }
}

