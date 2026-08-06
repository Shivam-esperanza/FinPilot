using Microsoft.Maui.Controls;
using System;
using FinPilot.Interfaces;

namespace FinPilot.Views
{
    public partial class DashboardPage : ContentPage
    {
        // 1. Class-Level Fields (Declared safely inside the class, NOT inside the constructor)
        private readonly IRecommendationEngine _engine;
        private readonly Guid _userId;

        // 2. The Page Constructor (Handles the Dependency Injection layout instantiation pipeline)
        public DashboardPage(IRecommendationEngine engine)
        {
            InitializeComponent();
            _engine = engine;

            // Generate a valid runtime GUID fallback context for your local testing look and feel
            _userId = Guid.NewGuid();
        }

        // 3. Independent Event Handler Method (Sits cleanly outside the constructor)
        private async void OnOpenAdvisorClicked(object sender, EventArgs e)
        {
            // Instantiates and navigates to the advisor view screen, passing your engine handles
            await Navigation.PushAsync(new PurchaseAdvisorPage(_engine, _userId));
        }
    }
}
