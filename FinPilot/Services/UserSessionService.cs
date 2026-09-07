using System;
using FinPilot.Interfaces;
using FinPilot.Models;

namespace FinPilot.Services
{
    public class UserSessionService : IUserSessionService
    {
        public User? CurrentUser { get; private set; }

        public Guid? CurrentUserId => CurrentUser?.Id;

        public bool IsLoggedIn => CurrentUser != null;

        public void SetUser(User user)
        {
            CurrentUser = user ?? throw new ArgumentNullException(nameof(user));
        }

        public void ClearSession()
        {
            CurrentUser = null;
        }
    }
}
