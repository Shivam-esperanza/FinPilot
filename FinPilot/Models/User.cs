using System;
using System.Collections.Generic;
using System.Text;

namespace FinPilot.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int CreditScore { get; set; }
        public decimal MonthlyIncome { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties for Entity Framework Core
        public ICollection<Account> Accounts { get; set; } = new List<Account>();
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
        public ICollection<CreditCard> CreditCards { get; set; } = new List<CreditCard>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
