using System.Threading.Tasks;
using FinPilot.Models;

namespace FinPilot.Interfaces
{
    public interface IAssistantService
    {
        Task<string> GenerateRecommendationExplanationAsync(User user, PurchaseCriteria criteria, AdvisorOption topOption);
    }
}
