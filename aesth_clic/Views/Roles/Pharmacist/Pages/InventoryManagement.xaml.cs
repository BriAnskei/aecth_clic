using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace aesth_clic.Views.Roles.Pharmacist.Pages
{
    public class InventoryItem
    {
        public string MedicineId { get; set; } = string.Empty;
        public string MedicineName { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string ExpiryDate { get; set; } = string.Empty;

        public string StatusLabel =>
            StockQuantity <= 10 ? "Restock Now" :
            StockQuantity <= 30 ? "Low Stock" :
            "Sufficient";

        public string StatusIcon =>
            StockQuantity <= 10 ? "\uE783" :
            StockQuantity <= 30 ? "\uE7BA" :
            "\uE73E";

        public string StatusTextColor =>
            StockQuantity <= 10 ? "#C0392B" :
            StockQuantity <= 30 ? "#B7580A" :
            "#2E7D32";
    }

    public sealed partial class InventoryManagement : Page
    {
        private List<InventoryItem> _allItems = new();

        public InventoryManagement()
        {
            InitializeComponent();
            LoadSampleData();
            Loaded += (_, _) => ApplyFilters();
        }

        private void LoadSampleData()
        {
            _allItems = new List<InventoryItem>
            {
                new() { MedicineId = "m01", MedicineName = "Lidocaine 2%",           StockQuantity = 150, Unit = "vial",    ExpiryDate = "Dec 2025" },
                new() { MedicineId = "m02", MedicineName = "Hyaluronic Acid Filler", StockQuantity =  28, Unit = "syringe", ExpiryDate = "Mar 2026" },
                new() { MedicineId = "m03", MedicineName = "Botulinum Toxin A",      StockQuantity =   8, Unit = "vial",    ExpiryDate = "Jun 2025" },
                new() { MedicineId = "m04", MedicineName = "Amoxicillin 500mg",      StockQuantity = 200, Unit = "capsule", ExpiryDate = "Jan 2026" },
                new() { MedicineId = "m05", MedicineName = "Clindamycin 300mg",      StockQuantity =   9, Unit = "capsule", ExpiryDate = "Sep 2025" },
                new() { MedicineId = "m06", MedicineName = "Ibuprofen 400mg",        StockQuantity = 300, Unit = "tablet",  ExpiryDate = "Aug 2026" },
                new() { MedicineId = "m07", MedicineName = "Tramadol 50mg",          StockQuantity =  25, Unit = "tablet",  ExpiryDate = "Nov 2025" },
                new() { MedicineId = "m08", MedicineName = "Vitamin C 500mg",        StockQuantity = 500, Unit = "tablet",  ExpiryDate = "Jul 2026" },
                new() { MedicineId = "m09", MedicineName = "Collagen Supplement",    StockQuantity =  22, Unit = "capsule", ExpiryDate = "Apr 2026" },
                new() { MedicineId = "m10", MedicineName = "Prilocaine 3%",          StockQuantity =   6, Unit = "vial",    ExpiryDate = "Oct 2025" },
                new() { MedicineId = "m11", MedicineName = "Calcium Gluconate",      StockQuantity =  30, Unit = "tablet",  ExpiryDate = "May 2026" },
                new() { MedicineId = "m12", MedicineName = "Mefenamic Acid 500mg",   StockQuantity = 120, Unit = "capsule", ExpiryDate = "Feb 2026" },
            };
        }

        private void ApplyFilters()
        {
            if (InventoryListControl is null) return;

            var search = SearchBox?.Text?.Trim().ToLower() ?? string.Empty;
            var statusFilter = (StatusFilter?.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All";

            var filtered = _allItems.Where(m =>
                (string.IsNullOrEmpty(search) || m.MedicineName.ToLower().Contains(search))
                && (statusFilter == "All" || m.StatusLabel == statusFilter)
            ).ToList();

            InventoryListControl.ItemsSource = filtered;

            if (TxtRowCount is not null)
                TxtRowCount.Text = $"Showing {filtered.Count} item{(filtered.Count == 1 ? "" : "s")}";
        }

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
            => ApplyFilters();

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => ApplyFilters();

        private async void AddMedicine_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Add Medicine",
                Content = "Add medicine functionality will be implemented later.",
                CloseButtonText = "Close",
                XamlRoot = XamlRoot
            };
            await dialog.ShowAsync();
        }

        private async void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            var id = (sender as MenuFlyoutItem)?.Tag?.ToString();
            var record = _allItems.FirstOrDefault(m => m.MedicineId == id);
            if (record is null) return;

            var dialog = new ContentDialog
            {
                Title = record.MedicineName,
                Content = "Detailed medicine information will be implemented later.",
                CloseButtonText = "Close",
                XamlRoot = XamlRoot
            };
            await dialog.ShowAsync();
        }

        private async void EditMedicine_Click(object sender, RoutedEventArgs e)
        {
            var id = (sender as MenuFlyoutItem)?.Tag?.ToString();
            var record = _allItems.FirstOrDefault(m => m.MedicineId == id);
            if (record is null) return;

            var dialog = new ContentDialog
            {
                Title = $"Edit — {record.MedicineName}",
                Content = "Edit medicine functionality will be implemented later.",
                CloseButtonText = "Close",
                XamlRoot = XamlRoot
            };
            await dialog.ShowAsync();
        }

        private async void DeleteMedicine_Click(object sender, RoutedEventArgs e)
        {
            var id = (sender as MenuFlyoutItem)?.Tag?.ToString();
            var record = _allItems.FirstOrDefault(m => m.MedicineId == id);
            if (record is null) return;

            var dialog = new ContentDialog
            {
                Title = "Delete Medicine",
                Content = $"Are you sure you want to delete {record.MedicineName}?\nThis functionality will be implemented later.",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}