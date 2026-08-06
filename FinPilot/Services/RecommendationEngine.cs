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

namespace FinPilot.Services
{
    public class RecommendationEngine : IRecommendationEngine
    {
        private readonly AppDbContext _dbContext;
        private readonly IBankOfferService _offerService;

        public RecommendationEngine(AppDbContext dbContext, IBankOfferService offerService)
        {
            _dbContext = dbContext;
            _offerService = offerService;
        }

        public async Task<List<AdvisorOption>> EvaluateFinancingOptionsAsync(PurchaseCriteria criteria)
        {
            var optionsResult = new List<AdvisorOption>();

            // 1. Gather all baseline user financial parameters
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == criteria.UserId);
            if (user == null) return optionsResult;

            var userCards = await _dbContext.CreditCards.Where(c => c.UserId == criteria.UserId).ToListAsync();
            var benchmarkRates = await _offerService.GetBenchmarkLoanRatesAsync();
            var activeOffers = await _offerService.GetActiveMarketOffersAsync();

            // 2. PATHWAY A: Evaluate Personal Loan Options across all benchmark banks
            foreach (var rate in benchmarkRates)
            {
                string bankName = rate.Key;
                decimal annualRate = rate.Value;

                // Basic loan metrics calculation
                decimal monthlyRate = (annualRate / 12m) / 100m;
                int tenure = criteria.PreferredTenureMonths > 0 ? criteria.PreferredTenureMonths : 12;

                // Amortization Formula: EMI = [P x R x (1+R)^N] / [((1+R)^N) - 1]
                double compoundFactor = Math.Pow((double)(1 + monthlyRate), tenure);
                decimal monthlyEmi = criteria.ProductPrice * monthlyRate * (decimal)compoundFactor / (decimal)(compoundFactor - 1);

                decimal totalCostPaid = monthlyEmi * tenure;
                decimal totalInterest = totalCostPaid - criteria.ProductPrice;
                decimal processingFee = criteria.ProductPrice * 0.01m; // Standard 1% administrative baseline processing fee

                var loanOption = new AdvisorOption
                {
                    BankName = bankName,
                    FinancingType = "PersonalLoan",
                    TenureMonths = tenure,
                    MonthlyEmi = Math.Round(monthlyEmi, 2),
                    InterestRate = annualRate,
                    TotalInterestPayable = Math.Round(totalInterest, 2),
                    ProcessingFee = Math.Round(processingFee, 2),
                    InstantCashback = 0.00m,
                    NetEffectiveCost = Math.Round(criteria.ProductPrice + totalInterest + processingFee, 2)
                };

                loanOption.MatchScore = CalculateScore(user, loanOption, userCards);
                loanOption.RecommendationReason = $"Structured personal loan via {bankName} at a competitive {annualRate}% annual benchmark APR.";
                optionsResult.Add(loanOption);
            }

            // 3. PATHWAY B: Evaluate Credit Card Financing Options (Including specialized No-Cost EMI campaigns)
            foreach (var card in userCards)
            {
                // Ensure card infrastructure matches active balance limit parameters
                if (card.AvailableLimit < criteria.ProductPrice) continue;

                int tenure = criteria.PreferredTenureMonths > 0 ? criteria.PreferredTenureMonths : 12;

                // Scan for active marketplace partnership campaigns (Feature 5)
                var noCostCampaign = activeOffers.FirstOrDefault(o => o.OfferType == "NoCostEmi" && o.IsActive);
                var flatCashbackCampaign = activeOffers.FirstOrDefault(o => o.OfferType == "InstantDiscount" && o.IsActive);

                decimal instantCashback = 0.00m;
                decimal totalInterest = 0.00m;
                decimal processingFee = 199.00m; // Standard flat clearing house card transaction operational processing fee

                if (noCostCampaign != null && card.Bank?.Name == "ICICI Bank" && tenure <= (int)noCostCampaign.ValueValue)
                {
                    // No-Cost EMI: Interest is discounted upfront by the merchant
                    instantCashback = criteria.ProductPrice * 0.05m; // Simulating upfront subvention discount
                    totalInterest = 0.00m;
                }
                else if (flatCashbackCampaign != null && card.Bank?.Name == "HDFC Bank")
                {
                    instantCashback = Math.Min(flatCashbackCampaign.ValueValue, flatCashbackCampaign.MaximumDiscount);
                    // Standard card commercial interest rate assumption (e.g., 14% APR for non-promotional EMIs)
                    totalInterest = criteria.ProductPrice * (0.14m / 12m) * tenure * 0.55m;
                }

                decimal monthlyEmi = (criteria.ProductPrice + totalInterest - instantCashback) / tenure;

                var cardOption = new AdvisorOption
                {
                    BankName = card.Bank?.Name ?? "Associated Bank",
                    FinancingType = noCostCampaign != null && card.Bank?.Name == "ICICI Bank" ? "NoCostEmi" : "CreditCardEmi",
                    TenureMonths = tenure,
                    MonthlyEmi = Math.Round(monthlyEmi, 2),
                    InterestRate = totalInterest > 0 ? 14.00m : 0.00m,
                    TotalInterestPayable = Math.Round(totalInterest, 2),
                    ProcessingFee = processingFee,
                    InstantCashback = instantCashback,
                    NetEffectiveCost = Math.Round(criteria.ProductPrice + totalInterest + processingFee - instantCashback, 2)
                };

                cardOption.MatchScore = CalculateScore(user, cardOption, userCards);
                cardOption.RecommendationReason = cardOption.FinancingType == "NoCostEmi"
                    ? $"Highly optimized zero-interest subvention campaign leveraging your active {card.CardName} account."
                    : $"Direct credit card utilization with an instant discount voucher of ₹{instantCashback}.";

                optionsResult.Add(cardOption);
            }

            // 4. Return an organized, ranked list sorted from highest score to lowest
            return optionsResult.OrderByDescending(o => o.MatchScore).ToList();
        }

        // Feature 6 Multi-Criteria Scoring Weight Matrix Engine Algorithm
        private double CalculateScore(User user, AdvisorOption option, List<CreditCard> userCards)
        {
            double baseScore = 70.0; // Starting baseline normalization constant

            // Parameter 1: Net Cost Performance (Up to +15 pts or -15 pts deviation adjustments)
            decimal priceToCostDelta = user.MonthlyIncome - option.MonthlyEmi;
            if (option.NetEffectiveCost < user.MonthlyIncome * 2) baseScore += 10;
            if (option.FinancingType == "NoCostEmi") baseScore += 10; // Instantly prioritize interest-free structures

            // Parameter 2: Institutional Relationship Bias (Feature 6 Mapping)
            // Prioritize banks the user already owns accounts/cards with
            bool hasPriorRelationship = userCards.Any(c => c.Bank?.Name == option.BankName);
            if (hasPriorRelationship)
            {
                baseScore += 8.0; // Dynamic preference bump for existing banking links
            }

            // Parameter 3: Risk Framework Boundaries / Credit Score Eligibility Checklist
            if (user.CreditScore >= 750) baseScore += 5.0; // High approval probability index addition
            else if (user.CreditScore < 680) baseScore -= 15.0; // Strained credit profile dampener

            // Parameter 4: Affordability Check
            decimal emiBurdenRatio = (option.MonthlyEmi / user.MonthlyIncome) * 100;
            if (emiBurdenRatio > 40m) baseScore -= 20.0; // Penalize options that breach safe debt limits

            return Math.Clamp(baseScore, 0.0, 100.0);
        }
    }
}
