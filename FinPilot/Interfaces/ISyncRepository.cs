using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FinPilot.Interfaces
{
    public interface ISyncRepository
    {
        // Scans local SQLite rows for unsynced changes and performs a safe data handshake with the cloud
        Task<bool> SynchronizeDataWithCloudAsync(Guid userId);

        // Checks internet network status boundaries on mobile hardware
        Task<bool> IsCloudEndpointAccessibleAsync();
    }
}
