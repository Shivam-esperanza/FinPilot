using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinPilot.Models;

namespace FinPilot.Interfaces
{
    public interface INotificationManagerService
    {
        // Audits all financial boundaries and generates smart contextual alerts
        Task RunBackgroundAuditAsync(Guid userId);

        // Fetches all notifications logged for a specific user
        Task<List<Notification>> GetUserNotificationsAsync(Guid userId);

        // Marks a specific logged notification as read
        Task MarkAsReadAsync(Guid notificationId);
    }
}
