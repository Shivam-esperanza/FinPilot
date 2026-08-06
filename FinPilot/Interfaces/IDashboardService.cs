using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FinPilot.Models;

namespace FinPilot.Interfaces
{
    public interface IDashboardService
    {
        // Computes and returns the complete analytical financial metrics for a user
        Task<DashboardSummary> GetDashboardSummaryAsync(Guid userId);
    }
}

