using System;
using System.Collections.Generic;
using System.Text;

namespace FinPilot.Models
{
    public class PurchaseCriteria
    {
        public Guid UserId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal ProductPrice { get; set; }
        public string ProductCategory { get; set; } = string.Empty; // e.g., "Laptop", "Smartphone"
        public int PreferredTenureMonths { get; set; } // The user's desired repayment window
    }
}
