using System;
using FinPilot.Models;

namespace FinPilot.Interfaces
{
    public interface IUserSessionService
    {
        User? CurrentUser { get; }
        Guid? CurrentUserId { get; }
        bool IsLoggedIn { get; }

        void SetUser(User user);
        void ClearSession();
    }
}
