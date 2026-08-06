using System;
using System.Collections.Generic;
using System.Text;

using System.Collections.Generic;
using System.Threading.Tasks;
using FinPilot.Models;

namespace FinPilot.Interfaces
{
    public interface IRecommendationEngine
    {
        // Evaluates a specific purchase scenario and returns an organized, ranked collection of financial options
        Task<List<AdvisorOption>> EvaluateFinancingOptionsAsync(PurchaseCriteria criteria);
    }
}

