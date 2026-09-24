using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinPilot.Models;

namespace FinPilot.Interfaces
{
    public interface IBankOfferService
    {
        // Fetches real-time bank promotional offers from online endpoints/feeds and updates local store
        Task<List<Offer>> SyncLatestOffersFromInternetAsync();

        // Retrieves active promotional offers across credit cards and payment instruments
        Task<List<Offer>> GetActiveMarketOffersAsync();

        // Retrieves current benchmark lending interest rates across prominent institutions
        Task<Dictionary<string, decimal>> GetBenchmarkLoanRatesAsync();
    }
}
