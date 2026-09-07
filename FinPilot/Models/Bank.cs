using System;
using System.Collections.Generic;
using System.Text;

namespace FinPilot.Models
{
    public class Bank
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty; // e.g., "ICICI", "Axis", "SBI"
        public string Code { get; set; } = string.Empty; // e.g., "ICICI01", "AXIS01"
        public bool IsPartnerBank { get; set; }

        // Navigation Properties for Entity Framework Core
        public ICollection<Account> Accounts { get; set; } = new List<Account>();
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
        public ICollection<CreditCard> CreditCards { get; set; } = new List<CreditCard>();
    }
}
