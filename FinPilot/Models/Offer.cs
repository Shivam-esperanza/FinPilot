using System;
using System.Collections.Generic;
using System.Text;

namespace FinPilot.Models
{
    public class Offer
    {
        public Guid Id { get; set; }
        public Guid BankId { get; set; }
        public string Title { get; set; } = string.Empty; // e.g., "No Cost EMI on Laptops"
        public string Description { get; set; } = string.Empty;
        public string OfferType { get; set; } = string.Empty; // e.g., Cashback, InstantDiscount, NoCostEmi
        public decimal MinimumTxnAmount { get; set; }
        public decimal MaximumDiscount { get; set; }
        public decimal ValueValue { get; set; } // e.g., 10 for 10% discount, 5000 for ₹5000 cashback
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; }

        // Navigation Properties for Entity Framework Core
        public Bank? Bank { get; set; }
    }
}

