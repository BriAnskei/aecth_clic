using System;
using System.Diagnostics;
using aesth_clic.Views.Roles.SuperAdmin.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace aesth_clic.Views.Roles.SuperAdmin.Modals
{
    public class ManageModulesResult
    {
        public string Tier { get; init; } = string.Empty;
    }

    public sealed partial class ManageModules : ContentDialog
    {
        private readonly UserItem _user;
        public ManageModulesResult? Result { get; private set; }
        private string _selectedTier = string.Empty;
        private static readonly string[] Tiers = { "Basic", "Standard", "Premium" };

        public ManageModules(UserItem user)
        {
            _user = user ?? throw new ArgumentNullException(nameof(user));
            InitializeComponent();
            SetupDialog();
        }

        private void SetupDialog()
        {
            ClientNameHint.Text = $"Editing module tier for: {_user.FullName}";

            // ← FIXED: use actual current tier instead of random mock
            string currentTier = _user.Tier switch
            {
                "basic" => "Basic",
                "standard" => "Standard",
                "premium" => "Premium",
                _ => "Basic",
            };
            SelectCard(currentTier);
        }

        private void TierCard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn)
                return;
            string clickedTier = btn.Tag?.ToString() ?? string.Empty;
            if (_selectedTier == clickedTier)
                return;
            SelectCard(clickedTier);
        }

        private void SelectCard(string tier)
        {
            _selectedTier = tier;
            SetCardSelected(CardBasic, _selectedTier == "Basic");
            SetCardSelected(CardStandard, _selectedTier == "Standard");
            SetCardSelected(CardPremium, _selectedTier == "Premium");
        }

        private void SetCardSelected(Button card, bool isSelected)
        {
            if (isSelected)
            {
                card.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 99, 177));
                card.BorderThickness = new Thickness(2);
                card.Background = new SolidColorBrush(Color.FromArgb(25, 0, 120, 212));
            }
            else
            {
                card.ClearValue(Button.BorderBrushProperty);
                card.ClearValue(Button.BackgroundProperty);
                card.BorderThickness = new Thickness(1.5);
            }
        }

        private void OnSaveClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Result = new ManageModulesResult { Tier = _selectedTier };

            Debug.WriteLine("─────────────────────────────────────");
            Debug.WriteLine("[ManageModules] Saved changes:");
            Debug.WriteLine($"  Client : {_user.FullName}");
            Debug.WriteLine($"  Tier   : {Result.Tier}");
            Debug.WriteLine("─────────────────────────────────────");
        }
    }
}
