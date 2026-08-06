using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FinPilot.Interfaces;
using FinPilot.Models;
using FinPilot.Database;

namespace FinPilot.Services
{
    public class MockAuthService : IAuthenticationService
    {
        private readonly AppDbContext _dbContext;
        private static Guid? _currentUserId; // Simulates active memory session token storage

        // Dependency Injection: Requesting our EF Core database handle automatically
        public MockAuthService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> IsUserLoggedInAsync()
        {
            await Task.Delay(800); // Simulate local state validation latency
            return _currentUserId.HasValue;
        }

        public async Task<User?> LoginWithEmailAsync(string email, string password)
        {
            await Task.Delay(2000); // Simulate remote network round-trip delay

            // Simple validation rule for our prototype development phase
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@") || password.Length < 6)
            {
                return null;
            }

            return await EnsureMockUserExistsAsync(email, "9876543210");
        }

        public async Task<User?> LoginWithGoogleAsync(string idToken)
        {
            await Task.Delay(2500); // Simulate secure external identity validation handshake

            if (string.IsNullOrWhiteSpace(idToken)) return null;

            return await EnsureMockUserExistsAsync("google.user@finpilot.com", "9999988888");
        }

        public async Task<User?> LoginWithPhoneAsync(string phoneNumber, string otpToken)
        {
            await Task.Delay(1800); // Simulate cellular network validation latency

            if (phoneNumber.Length < 10 || otpToken != "123456") // Mock OTP validation token passcode
            {
                return null;
            }

            return await EnsureMockUserExistsAsync("phone.user@finpilot.com", phoneNumber);
        }

        public async Task LogoutAsync()
        {
            await Task.Delay(500);
            _currentUserId = null;
        }

        public async Task<User?> GetCurrentCurrentUserAsync()
        {
            if (!_currentUserId.HasValue) return null;

            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == _currentUserId.Value);
        }

        // Core Helper: Ensures our database has a robust profile to power our features
        private async Task<User> EnsureMockUserExistsAsync(string email, string phone)
        {
            // 1. Verify if the master user profile is already created
            var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser != null)
            {
                _currentUserId = existingUser.Id;
                return existingUser;
            }

            // 2. Instantiate high-fidelity baseline User Profile data
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                FullName = "Lovkush Jangid",
                Email = email,
                PhoneNumber = phone,
                CreditScore = 765,           // Solid initial CIBIL profile
                MonthlyIncome = 125000.00m,  // ₹1.25 Lakhs per month parameter
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.Users.AddAsync(newUser);

            // 3. Instantiate a concrete, prominent Indian Partner Bank entity (ICICI Bank)
            var partnerBank = new Bank
            {
                Id = Guid.NewGuid(),
                Name = "ICICI Bank",
                Code = "ICICI01",
                IsPartnerBank = true
            };
            await _dbContext.Banks.AddAsync(partnerBank);

            // 4. Seed an Active Loan record to test our auto-decay tracking engine (Feature 3)
            var mockLoan = new Loan
            {
                Id = Guid.NewGuid(),
                UserId = newUser.Id,
                BankId = partnerBank.Id,
                LoanNumber = "LN-ICICI-9921",
                LoanType = "Personal",
                TotalLoanAmount = 1000000.00m,   // ₹10 Lakhs original loan
                RemainingPrincipal = 850000.00m, // The starting point to check value decay later
                InterestRate = 10.50m,           // 10.5% APR interest profile
                TotalTenureMonths = 60,
                RemainingTenureMonths = 51,
                MonthlyEmi = 21494.00m,          // Flat computed EMI 
                NextEmiDate = DateTime.UtcNow.AddDays(5),
                LoanStartDate = DateTime.UtcNow.AddMonths(-9),
                MissedPaymentsCount = 0,
                PredictedClosureDate = DateTime.UtcNow.AddMonths(51),
                IsEligibleForRefinancing = false
            };
            await _dbContext.Loans.AddAsync(mockLoan);

            // 5. Seed a high-utility Credit Card to run our Dashboard tracking mathematics (Feature 4)
            var mockCard = new CreditCard
            {
                Id = Guid.NewGuid(),
                UserId = newUser.Id,
                BankId = partnerBank.Id,
                CardName = "ICICI Rubyx",
                LastFourDigits = "4321",
                CreditLimit = 300000.00m,       // ₹3 Lakh limit
                AvailableLimit = 225000.00m,
                OutstandingAmount = 75000.00m,  // 25% starting utilization index
                StatementDate = DateTime.UtcNow.AddDays(-10),
                DueDate = DateTime.UtcNow.AddDays(10),
                RewardPointsBalance = 4250m,
                TotalCashbackEarned = 1250.00m,
                MinimumPayment = 3750.00m,
                AnnualFee = 0.00m,              // Lifetime free setup parameters
                AnnualFeeDueDate = DateTime.UtcNow.AddMonths(3)
            };
            await _dbContext.CreditCards.AddAsync(mockCard);

            // 6. Commit every structural asset directly into the physical SQLite local file
            await _dbContext.SaveChangesAsync();

            _currentUserId = newUser.Id;
            return newUser;
        }

    }
}

