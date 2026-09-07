using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FinPilot.Interfaces;
using FinPilot.Models;
using FinPilot.Database;
using FinPilot.Services;

namespace FinPilot.Services
{
    public class NotificationManagerService : INotificationManagerService
    {
        private readonly AppDbContext _dbContext;

        public NotificationManagerService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task RunBackgroundAuditAsync(Guid userId)
        {
            DateTime todayDate = DateTime.UtcNow.Date;
            DateTime tomorrowDate = todayDate.AddDays(1);
            DateTime nowUtc = DateTime.UtcNow;
            bool alertsGenerated = false;

            // 1. AUDIT TYPE A: Scan for near-term Loan EMIs (Feature 8)
            var upcomingLoans = await _dbContext.Loans
                .Where(l => l.UserId == userId && l.RemainingPrincipal > 0)
                .ToListAsync();

            foreach (var loan in upcomingLoans)
            {
                int daysToEmi = (loan.NextEmiDate.Date - todayDate).Days;

                // Trigger alert precisely if the loan payment date is tomorrow (1 day away)
                if (daysToEmi == 1)
                {
                    bool alertExists = await _dbContext.Notifications.AnyAsync(n =>
                        n.UserId == userId && n.Type == "EmiReminder" && n.CreatedAt >= todayDate && n.CreatedAt < tomorrowDate);

                    if (!alertExists)
                    {
                        string loanNo = !string.IsNullOrWhiteSpace(loan.LoanNumber) ? loan.LoanNumber : "Loan";
                        await _dbContext.Notifications.AddAsync(new Notification
                        {
                            Id = Guid.NewGuid(),
                            UserId = userId,
                            Title = "Loan EMI Due Tomorrow",
                            Message = $"Your monthly EMI of ₹{loan.MonthlyEmi:N2} for loan {loanNo} will be auto-debited tomorrow.",
                            Type = "EmiReminder",
                            IsRead = false,
                            CreatedAt = nowUtc
                        });
                        alertsGenerated = true;
                    }
                }
            }

            // 2. AUDIT TYPE B: Scan for Credit Card Utilization Violations (Feature 4 & 8)
            var activeCards = await _dbContext.CreditCards.Include(c => c.Bank).Where(c => c.UserId == userId).ToListAsync();
            foreach (var card in activeCards)
            {
                // Trigger alert if card utilization crosses the strict 30% threshold
                if (card.CreditUtilizationPercentage > 30.0)
                {
                    string lastDigits = card.LastFourDigits ?? string.Empty;
                    bool alertExists = await _dbContext.Notifications.AnyAsync(n =>
                        n.UserId == userId && n.Type == "CardDue" && n.CreatedAt >= todayDate && n.CreatedAt < tomorrowDate &&
                        (string.IsNullOrEmpty(lastDigits) || (n.Message != null && n.Message.Contains(lastDigits))));

                    if (!alertExists)
                    {
                        await _dbContext.Notifications.AddAsync(new Notification
                        {
                            Id = Guid.NewGuid(),
                            UserId = userId,
                            Title = "High Credit Utilization Warning",
                            Message = $"Your card ending in {lastDigits} has crossed a 30% utilization ratio (Current: {card.CreditUtilizationPercentage:F1}%). This may negatively affect your credit score.",
                            Type = "CardDue",
                            IsRead = false,
                            CreatedAt = nowUtc
                        });
                        alertsGenerated = true;
                    }
                }
            }

            // 3. AUDIT TYPE C: Evaluate Optimization / Refinancing Opportunities
            foreach (var loan in upcomingLoans)
            {
                // If interest rates drop or user has high interest, simulate a refinance recommendation saving trigger
                if (loan.InterestRate > 10.0m)
                {
                    bool alertExists = await _dbContext.Notifications.AnyAsync(n =>
                        n.UserId == userId && n.Type == "OfferAlert");

                    if (!alertExists)
                    {
                        string loanNo = !string.IsNullOrWhiteSpace(loan.LoanNumber) ? loan.LoanNumber : "Loan";
                        decimal simulatedSavings = loan.RemainingPrincipal * 0.02m; // Predict roughly 2% savings profile index
                        await _dbContext.Notifications.AddAsync(new Notification
                        {
                            Id = Guid.NewGuid(),
                            UserId = userId,
                            Title = "Refinance Optimization Available",
                            Message = $"Smart Check: You can potentially save up to ₹{simulatedSavings:N0} by refinancing your loan {loanNo} to a lower interest partner framework.",
                            Type = "OfferAlert",
                            IsRead = false,
                            CreatedAt = nowUtc
                        });
                        alertsGenerated = true;
                    }
                }
            }

            // 4. Flush fresh alert entities straight into SQLite
            if (alertsGenerated)
            {
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(Guid userId)
        {
            return await _dbContext.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            var notification = await _dbContext.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
