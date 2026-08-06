using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using FinPilot.Interfaces;
using FinPilot.Models;

namespace FinPilot.Views
{
    public partial class PurchaseAdvisorPage : ContentPage
    {
        private readonly IRecommendationEngine _engine;
        private readonly Guid _userId;

        public PurchaseAdvisorPage(IRecommendationEngine engine, Guid userId)
        {
            InitializeComponent();
            _engine = engine;
            _userId = userId;
        }

        private async void OnAnalyzeClicked(object sender, EventArgs e)
        {
            if (!decimal.TryParse(PriceEntry.Text, out decimal price)) return;

            var criteria = new PurchaseCriteria
            {
                UserId = _userId,
                ProductPrice = price,
                ProductCategory = "Laptop",
                PreferredTenureMonths = 12
            };

            // Run the local multi-criteria calculations instantly
            List<AdvisorOption> results = await _engine.EvaluateFinancingOptionsAsync(criteria);
            ResultsCollection.ItemsSource = results;
        }
    }
}
