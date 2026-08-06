using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FinPilot.Interfaces
{
    public interface ILoanTrackingService
    {
        // Iterates through active liabilities and catches up the balances based on elapsed time
        Task AutoUpdateBalancesAsync(Guid userId);
    }
}
