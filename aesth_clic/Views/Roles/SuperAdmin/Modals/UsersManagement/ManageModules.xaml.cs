using aesth_clic.Views.Roles.SuperAdmin.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Diagnostics;
using Windows.UI;

namespace aesth_clic.Views.Roles.SuperAdmin.Modals
{
    // ─────────────────────────────────────────────────────────
    // RESULT MODEL
    // ─────────────────────────────────────────────────────────
    public class ManageModulesResult
    {
        /// <summary>
        /// The selected tier: "Basic", "Standard", or "Premium".
        /// Always has a value — a tier must be selected at all times.
        /// </summary>
        public string Tier { get; init; } = string.Empty;
    }

    // ─────────────────────────────────────────────────────────
    // DIALOG CODE-BEHIND
    // ─────────────────────────────────────────────────────────
    public sealed partial class ManageModules : ContentDialog
    {
        private readonly UserItem _user;

        public ManageModulesResult? Result { get; private set; }

        // Always has a value — no deselect allowed
        private string _selectedTier = string.Empty;

        private static readonly string[] Tiers = { "Basic", "Standard", "Premium" };

        // ── Constructor ────────────────────────────────────────
        public ManageModules(UserItem user)
        {
            _user = user ?? throw new ArgumentNullException(nameof(user));
            InitializeComponent();
            SetupDialog();
        }

        // ── Initial state ──────────────────────────────────────
        private void SetupDialog()
        {
            ClientNameHint.Text = $"Editing module tier for: {_user.FullName}";

            // Mock: random tier each open. Replace with _user.Tier when DB is wired.
            string mockCurrentTier = Tiers[new Random().Next(Tiers.Length)];
            SelectCard(mockCurrentTier);
        }

        // ── Tier card click ────────────────────────────────────
        private void TierCard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            string clickedTier = btn.Tag?.ToString() ?? string.Empty;

            // Already selected — do nothing, no deselect
            if (_selectedTier == clickedTier) return;

            SelectCard(clickedTier);
        }

        // ── Selection helpers (mirrors AddNewClient.SetCardSelected) ──
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

        // ── Save handler ───────────────────────────────────────
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