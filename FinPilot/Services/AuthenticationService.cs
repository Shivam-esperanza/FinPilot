using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FinPilot.Interfaces;
using FinPilot.Models;
using FinPilot.Database;

namespace FinPilot.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserSessionService _sessionService;

        public AuthenticationService(AppDbContext dbContext, IUserSessionService sessionService)
        {
            _dbContext = dbContext;
            _sessionService = sessionService;
        }

        public async Task<bool> IsUserLoggedInAsync()
        {
            await EnsureDefaultUserSeededAsync();
            return _sessionService.IsLoggedIn;
        }

        public async Task<User?> LoginWithEmailAsync(string email, string password)
        {
            await EnsureDefaultUserSeededAsync();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            string normalizedEmail = email.Trim().ToLowerInvariant();

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
            if (user == null)
            {
                return null;
            }

            bool isPasswordValid = PasswordHasher.VerifyPassword(password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return null;
            }

            _sessionService.SetUser(user);
            await SeedUserFinancialDataAsync(user.Id);
            return user;
        }

        public async Task<User?> RegisterWithEmailAsync(string fullName, string email, string password, string phoneNumber = "", decimal monthlyIncome = 0, int creditScore = 750)
        {
            await EnsureDefaultUserSeededAsync();

            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@") || string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                return null;
            }

            string normalizedEmail = email.Trim().ToLowerInvariant();

            bool emailExists = await _dbContext.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail);
            if (emailExists)
            {
                return null;
            }

            string hashedPassword = PasswordHasher.HashPassword(password);

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                FullName = string.IsNullOrWhiteSpace(fullName) ? "FinPilot User" : fullName.Trim(),
                Email = normalizedEmail,
                PasswordHash = hashedPassword,
                PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? "9876543210" : phoneNumber.Trim(),
                CreditScore = creditScore > 0 ? creditScore : 750,
                MonthlyIncome = monthlyIncome >= 0 ? monthlyIncome : 0m,
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.Users.AddAsync(newUser);
            await SeedUserFinancialDataAsync(newUser.Id);
            await _dbContext.SaveChangesAsync();

            _sessionService.SetUser(newUser);
            return newUser;
        }

        public async Task<User?> LoginWithGoogleAsync(string idToken)
        {
            await EnsureDefaultUserSeededAsync();
            if (string.IsNullOrWhiteSpace(idToken)) return null;

            string email = "google.user@finpilot.com";
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                user = await RegisterWithEmailAsync("Google User", email, "GoogleAuthSecret123!");
            }

            if (user != null)
            {
                _sessionService.SetUser(user);
            }

            return user;
        }

        public async Task<User?> LoginWithPhoneAsync(string phoneNumber, string otpToken)
        {
            await EnsureDefaultUserSeededAsync();
            if (phoneNumber.Length < 10 || otpToken != "123456") return null;

            string email = "phone.user@finpilot.com";
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                user = await RegisterWithEmailAsync("Phone User", email, "PhoneAuthSecret123!");
            }

            if (user != null)
            {
                _sessionService.SetUser(user);
            }

            return user;
        }

        public async Task LogoutAsync()
        {
            await Task.CompletedTask;
            _sessionService.ClearSession();
        }

        public async Task<User?> GetCurrentCurrentUserAsync()
        {
            await EnsureDefaultUserSeededAsync();
            if (!_sessionService.IsLoggedIn) return null;

            return _sessionService.CurrentUser;
        }

        // Ensures DB contains a default user if completely empty upon fresh initialization
        // Ensures DB contains a default user if completely empty upon fresh initialization
        private async Task EnsureDefaultUserSeededAsync()
        {
            // Migrate any existing old user record to Shivam Vashisth
            var legacyUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email.Contains("lovkush") || u.FullName.Contains("Lovkush"));
            if (legacyUser != null)
            {
                legacyUser.FullName = "Shivam Vashisth";
                legacyUser.Email = "shivam.vashisth@finpilot.com";
                await _dbContext.SaveChangesAsync();

                if (_sessionService.CurrentUser != null && _sessionService.CurrentUser.Id == legacyUser.Id)
                {
                    _sessionService.SetUser(legacyUser);
                }
            }

            bool hasUsers = await _dbContext.Users.AnyAsync();
            if (!hasUsers)
            {
                string defaultEmail = "shivam.vashisth@finpilot.com";
                string defaultPassword = "Password123!";

                var defaultUser = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = "Shivam Vashisth",
                    Email = defaultEmail,
                    PasswordHash = PasswordHasher.HashPassword(defaultPassword),
                    PhoneNumber = "9876543210",
                    CreditScore = 765,
                    MonthlyIncome = 125000.00m,
                    CreatedAt = DateTime.UtcNow
                };

                await _dbContext.Users.AddAsync(defaultUser);
                await SeedUserFinancialDataAsync(defaultUser.Id);
                await _dbContext.SaveChangesAsync();
            }

            // Ensure current active user has baseline bank account, loan, and card seeded
            if (_sessionService.IsLoggedIn && _sessionService.CurrentUserId.HasValue)
            {
                await SeedUserFinancialDataAsync(_sessionService.CurrentUserId.Value);
            }
        }

        private async Task SeedUserFinancialDataAsync(Guid userId)
        {
            var partnerBank = await _dbContext.Banks.FirstOrDefaultAsync(b => b.Code == "ICICI01");
            if (partnerBank == null)
            {
                partnerBank = new Bank
                {
                    Id = Guid.NewGuid(),
                    Name = "ICICI Bank",
                    Code = "ICICI01",
                    IsPartnerBank = true
                };
                await _dbContext.Banks.AddAsync(partnerBank);
                await _dbContext.SaveChangesAsync();
            }

            bool hasAccounts = await _dbContext.Accounts.AnyAsync(a => a.UserId == userId);
            if (!hasAccounts)
            {
                var mockAccount = new Account
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    BankId = partnerBank.Id,
                    AccountNumber = "ACC-ICICI-9876",
                    AccountType = "Salary",
                    Balance = 150000.00m,
                    IsPrimaryRelationship = true
                };
                await _dbContext.Accounts.AddAsync(mockAccount);
            }

            bool hasLoans = await _dbContext.Loans.AnyAsync(l => l.UserId == userId);
            if (!hasLoans)
            {
                var mockLoan = new Loan
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    BankId = partnerBank.Id,
                    LoanNumber = "LN-ICICI-9921",
                    LoanType = "Personal",
                    TotalLoanAmount = 1000000.00m,
                    RemainingPrincipal = 850000.00m,
                    InterestRate = 10.50m,
                    TotalTenureMonths = 60,
                    RemainingTenureMonths = 51,
                    MonthlyEmi = 21494.00m,
                    NextEmiDate = DateTime.UtcNow.AddDays(5),
                    LoanStartDate = DateTime.UtcNow.AddMonths(-9),
                    MissedPaymentsCount = 0,
                    PredictedClosureDate = DateTime.UtcNow.AddMonths(51),
                    IsEligibleForRefinancing = false
                };
                await _dbContext.Loans.AddAsync(mockLoan);
            }

            bool hasCards = await _dbContext.CreditCards.AnyAsync(c => c.UserId == userId);
            if (!hasCards)
            {
                var mockCard = new CreditCard
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    BankId = partnerBank.Id,
                    CardName = "ICICI Rubyx",
                    LastFourDigits = "4321",
                    CreditLimit = 300000.00m,
                    AvailableLimit = 225000.00m,
                    OutstandingAmount = 75000.00m,
                    StatementDate = DateTime.UtcNow.AddDays(-10),
                    DueDate = DateTime.UtcNow.AddDays(10),
                    RewardPointsBalance = 4250m,
                    TotalCashbackEarned = 1250.00m,
                    MinimumPayment = 3750.00m,
                    AnnualFee = 0.00m,
                    AnnualFeeDueDate = DateTime.UtcNow.AddMonths(3)
                };
                await _dbContext.CreditCards.AddAsync(mockCard);
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
