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
        public string PhoneNumber { get; set; } = string.Empty;
        public int CreditScore { get; set; }
        public decimal MonthlyIncome { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
