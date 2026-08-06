using System;
using System.Collections.Generic;
using System.Text;

namespace FinPilot.Models
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? LoanId { get; set; }        // Nullable: Transaction might be for a credit card
        public Guid? CreditCardId { get; set; }  // Nullable: Transaction might be for a loan payment
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; } = string.Empty; // e.g., "EMI Auto-Debited" or "Amazon Purchase"
        public string Category { get; set; } = string.Empty;    // e.g., LoanPayment, Shopping, Fuel

        // Navigation Properties
        public User? User { get; set; }
        public Loan? Loan { get; set; }
        public CreditCard? CreditCard { get; set; }
    }
}

