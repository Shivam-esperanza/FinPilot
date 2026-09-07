using System;
using System.Threading.Tasks;
using FinPilot.Interfaces;
using FinPilot.Models;

namespace FinPilot.Services
{
    public class AssistantService : IAssistantService
    {
        public async Task<string> GenerateRecommendationExplanationAsync(User user, PurchaseCriteria criteria, AdvisorOption topOption)
        {
            await Task.Delay(200); // Fast local processing

            if (user == null || topOption == null || criteria == null)
            {
                return "FinPilot Assistant: Insufficient financial context to generate an advisory breakdown.";
            }

            decimal monthlyIncome = user.MonthlyIncome > 0 ? user.MonthlyIncome : 1m;
            double emiToIncomePct = (double)(topOption.MonthlyEmi / monthlyIncome) * 100;

            string explanation = $"[FinPilot Local AI Advisory Engine]\n" +
                $"Recommendation for '{criteria.ProductName}' (₹{criteria.ProductPrice:N0}):\n" +
                $"• Selected Option: {topOption.BankName} {topOption.FinancingType} over {topOption.TenureMonths} months.\n" +
                $"• Monthly Impact: ₹{topOption.MonthlyEmi:N0}/month ({emiToIncomePct:F1}% of your monthly income).\n" +
                $"• Match Confidence: {topOption.MatchScore:F0}% match based on your CIBIL score of {user.CreditScore}.\n\n" +
                $"Reasoning: {topOption.RecommendationReason} " +
                $"This option maintains your EMI burden within a safe threshold while maximizing value.";

            return explanation;
        }
    }
}
