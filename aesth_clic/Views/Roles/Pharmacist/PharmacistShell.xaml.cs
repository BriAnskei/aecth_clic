using aesth_clic.Session;
using aesth_clic.Views.Roles.Pharmacist.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;

namespace aesth_clic.Views.Roles
{
    public sealed partial class PharmacistShell : Page
    {
        public PharmacistShell()
        {
            InitializeComponent();
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyTierVisibility();

            string tier = (AppSession.Instance.CurrentClient?.Tier
                           ?? throw new InvalidOperationException("No active client in session."))
                          .ToLowerInvariant();

            if (tier == "basic" || tier == "standard")
            {
                NavView.SelectedItem = NavView.MenuItems[1]; // Inventory
                ContentFrame.Navigate(typeof(InventoryManagement));
            }
            else
            {
                NavView.SelectedItem = NavView.MenuItems[0]; // Patient Medicine
                ContentFrame.Navigate(typeof(PatientMedicine));
            }
        }

        private void ApplyTierVisibility()
        {
            string tier = (AppSession.Instance.CurrentClient?.Tier
                           ?? throw new InvalidOperationException("No active client in session."))
                          .ToLowerInvariant();

            var patientMedicine = (NavigationViewItem)NavView.MenuItems[0]; // Patient Medicine
            var inventory = (NavigationViewItem)NavView.MenuItems[1]; // Inventory

            switch (tier)
            {
                case "basic":
                case "standard":
                    patientMedicine.Visibility = Visibility.Collapsed;
                    inventory.Visibility = Visibility.Visible;
                    break;

                case "premium":
                    patientMedicine.Visibility = Visibility.Visible;
                    inventory.Visibility = Visibility.Visible;
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unrecognized clinic tier: '{tier}'. Expected 'basic', 'standard', or 'premium'.");
            }
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item)
            {
                string tag = item.Tag?.ToString() ?? string.Empty;
                switch (tag)
                {
                    case "PatientMedicine":
                        ContentFrame.Navigate(typeof(PatientMedicine));
                        break;
                    case "Inventory":
                        ContentFrame.Navigate(typeof(InventoryManagement));
                        break;
                }
            }
        }

        private void OnLogout(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(RoleSelectionPage));
        }
    }
}