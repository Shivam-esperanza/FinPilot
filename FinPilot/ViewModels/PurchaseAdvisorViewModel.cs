using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using FinPilot.Database;
using FinPilot.Interfaces;
using FinPilot.Models;

namespace FinPilot.ViewModels
{
    public partial class PurchaseAdvisorViewModel : ObservableObject
    {
        private readonly IRecommendationEngine _engine;
        private readonly IAssistantService _assistantService;
        private readonly AppDbContext _dbContext;
        private readonly IUserSessionService _sessionService;

        [ObservableProperty]
        private string _productName = string.Empty;

        [ObservableProperty]
        private string _productPriceText = string.Empty;

        [ObservableProperty]
        private string _productCategory = "Electronics"; // Electronics, Appliance, Vehicle, General

        [ObservableProperty]
        private string _tenureText = "12";

        [ObservableProperty]
        private ObservableCollection<AdvisorOption> _results = new();

        [ObservableProperty]
        private string _aiExplanation = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public PurchaseAdvisorViewModel(
            IRecommendationEngine engine,
            IAssistantService assistantService,
            AppDbContext dbContext,
            IUserSessionService sessionService)
        {
            _engine = engine;
            _assistantService = assistantService;
            _dbContext = dbContext;
            _sessionService = sessionService;
        }

        [RelayCommand]
        private async Task AnalyzePurchaseAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;
                AiExplanation = string.Empty;
                Results.Clear();

                Guid userId = _sessionService.CurrentUserId ?? Guid.Empty;
                if (userId == Guid.Empty)
                {
                    ErrorMessage = "User session invalid. Please log in.";
                    return;
                }

                if (!decimal.TryParse(ProductPriceText, out decimal price) || price <= 0)
                {
                    ErrorMessage = "Please enter a valid positive product price.";
                    return;
                }

                if (!int.TryParse(TenureText, out int tenure) || tenure <= 0)
                {
                    tenure = 12;
                }

                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    ErrorMessage = "User profile not found.";
                    return;
                }

                var criteria = new PurchaseCriteria
                {
                    UserId = userId,
                    ProductPrice = price,
                    ProductName = string.IsNullOrWhiteSpace(ProductName) ? "Desired Purchase" : ProductName.Trim(),
                    ProductCategory = ProductCategory,
                    PreferredTenureMonths = tenure
                };

                var options = await _engine.EvaluateFinancingOptionsAsync(criteria);
                Results = new ObservableCollection<AdvisorOption>(options);

                if (options.Count > 0)
                {
                    var topOption = options[0];
                    AiExplanation = await _assistantService.GenerateRecommendationExplanationAsync(user, criteria, topOption);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Analysis error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
