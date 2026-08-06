using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinPilot.Models;

namespace FinPilot.Interfaces
{
    public interface IAuthenticationService
    {
        // Check if an authenticated user session is already preserved locally
        Task<bool> IsUserLoggedInAsync();

        // Standard corporate email identity authentication routine
        Task<User?> LoginWithEmailAsync(string email, string password);

        // Third-party identity broker authentication routine
        Task<User?> LoginWithGoogleAsync(string idToken);

        // Secure carrier/mobile network cellular authentication routine
        Task<User?> LoginWithPhoneAsync(string phoneNumber, string otpToken);

        // Safely invalidate active profile security contexts
        Task LogoutAsync();

        // Retrieve the cached session context data of the currently logged-in account
        Task<User?> GetCurrentCurrentUserAsync();
    }
}

