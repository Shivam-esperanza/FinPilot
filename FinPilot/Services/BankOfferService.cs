using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinPilot.Interfaces;
using FinPilot.Models;

namespace FinPilot.Services
{
    public class BankOfferService : IBankOfferService
    {
        public async Task<List<Offer>> GetActiveMarketOffersAsync()
        {
            await Task.Delay(400); // Simulate local data load latency or swift API pull

            // Hardcoding structural campaigns matching your precise product ideas
            return new List<Offer>
            {
                new Offer
                {
                    Id = Guid.NewGuid(),
                    Title = "Flipkart + SBI Card Promotion",
                    Description = "10% Instant Cashback on minimum transactions of ₹15,000 using SBI Credit Cards.",
                    OfferType = "Cashback",
                    MinimumTxnAmount = 15000.00m,
                    MaximumDiscount = 5000.00m,
                    ValueValue = 10.00m, // 10% rate representation
                    ExpiryDate = DateTime.UtcNow.AddDays(15),
                    IsActive = true
                },
                new Offer
                {
                    Id = Guid.NewGuid(),
                    Title = "Amazon + ICICI No-Cost EMI",
                    Description = "12 Months No-Cost EMI tenure configuration matching Laptop purchases.",
                    OfferType = "NoCostEmi",
                    MinimumTxnAmount = 30000.00m,
                    MaximumDiscount = 8000.00m,
                    ValueValue = 12.00m, // 12 Months duration parameter
                    ExpiryDate = DateTime.UtcNow.AddDays(25),
                    IsActive = true
                },
                new Offer
                {
                    Id = Guid.NewGuid(),
                    Title = "Reliance Digital + HDFC Instant Saver",
                    Description = "Flat ₹10,000 Instant Discount structural credit asset voucher.",
                    OfferType = "InstantDiscount",
                    MinimumTxnAmount = 100000.00m,
                    MaximumDiscount = 10000.00m,
                    ValueValue = 10000.00m, // Flat ₹10,000 cash value deduction
                    ExpiryDate = DateTime.UtcNow.AddDays(8),
                    IsActive = true
                }
            };
        }

        public async Task<Dictionary<string, decimal>> GetBenchmarkLoanRatesAsync()
        {
            await Task.Delay(200);

            // Seed standardized, dynamic personal loan base annual interest percentages (APR)
            return new Dictionary<string, decimal>
            {
                { "ICICI Bank", 9.10m },   // Matching your competitive preference criteria
                { "HDFC Bank", 10.25m },
                { "SBI", 9.50m },
                { "Axis Bank", 10.75m },
                { "Kotak Bank", 11.00m }
            };
        }
    }
}

