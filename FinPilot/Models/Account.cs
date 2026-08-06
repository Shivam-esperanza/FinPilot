using System;
using System.Collections.Generic;
using System.Text;


namespace FinPilot.Models
{
    public class Account
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid BankId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty; // Savings, Salary, Current
        public decimal Balance { get; set; }
        public bool IsPrimaryRelationship { get; set; } // Helps recommendation engine prioritize

        // Navigation Properties for Entity Framework Core relationship mapping later
        public User? User { get; set; }
        public Bank? Bank { get; set; }
    }
}

