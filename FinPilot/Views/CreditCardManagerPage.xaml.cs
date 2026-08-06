using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using FinPilot.Models;

namespace FinPilot.Views
{
    public partial class CreditCardManagerPage : ContentPage
    {
        public CreditCardManagerPage(List<CreditCard> userCards)
        {
            InitializeComponent();

            // Assigning data collection items directly to our compiled list frame elements
            CardsCollection.ItemsSource = userCards;
        }
    }
}
