using aesth_clic.Tenant.Controller;
using aesth_clic.Utils;
using aesth_clic.ViewModels.Pharmacist;
using aesth_clic.Views.Roles.Pharmacist.Modals;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;

namespace aesth_clic.Views.Roles.Pharmacist.Pages
{
    // ── UI display model ───────────────────────────────────────────────────────────
    public class InventoryItem
    {
        public string MedicineId { get; set; } = string.Empty;
        public string MedicineName { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public string LastStockIn { get; set; } = string.Empty;
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

    // ── Page ───────────────────────────────────────────────────────────────────────
    public sealed partial class InventoryManagement : Page
    {
        private readonly InventoryManagementViewModel _vm = new();
        private readonly MedicineController _controller;

        public InventoryManagement()
        {
            InitializeComponent();

            _controller = App.Services.GetRequiredService<MedicineController>();

            InventoryListControl.ItemsSource = _vm.DisplayedItems;

            _vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName
                    is nameof(InventoryManagementViewModel.TotalMedicines)
                    or nameof(InventoryManagementViewModel.SufficientCount)
                    or nameof(InventoryManagementViewModel.LowStockCount))
                    UpdateKpiCards();
            };

            _ = LoadFromDbAsync();
        }

        // ── Data loading ───────────────────────────────────────────────────────────
        private async System.Threading.Tasks.Task LoadFromDbAsync()
        {
            try
            {
                var medicines = await _controller.GetAllMedicinesAsync();

                _vm.LoadFromDb(medicines.Select(m => (
                    MedicineId: m.Id.ToString(),
                    MedicineName: m.Name,
                    StockQuantity: m.Stock,
                    LastStockIn: m.LastStockIn.ToLocalTime().ToString("MMM yyyy"),
                    Unit: m.Unit,
                    ExpiryDate: m.ExpiryDate.ToLocalTime().ToString("MMM yyyy")
                )));

                InventoryListControl.ItemsSource = null;
                InventoryListControl.ItemsSource = _vm.DisplayedItems;

                UpdateKpiCards();
            }
            catch (Exception ex)
            {
                ToastHelper.Error(ToastBar, "Failed to load inventory", ex.Message);
            }
        }

        // ── KPI Cards ──────────────────────────────────────────────────────────────
        private void UpdateKpiCards()
        {
            if (TxtTotalMedicines is null || TxtSufficientCount is null ||
                TxtLowStockCount is null || TxtRowCount is null)
                return;

            TxtTotalMedicines.Text = _vm.TotalMedicines.ToString();
            TxtSufficientCount.Text = _vm.SufficientCount.ToString();
            TxtLowStockCount.Text = _vm.LowStockCount.ToString();
            TxtRowCount.Text =
                $"Showing {_vm.DisplayedItems.Count} item{(_vm.DisplayedItems.Count != 1 ? "s" : "")}";
        }

        // ── Search + Filters ───────────────────────────────────────────────────────
        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            _vm.SearchText = sender.Text ?? string.Empty;
            UpdateKpiCards();
        }

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _vm.SelectedStatus =
                (StatusFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All";
            UpdateKpiCards();
        }

        // ── Add Medicine ───────────────────────────────────────────────────────────
        private async void AddMedicine_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddEditMedicine(_controller) { XamlRoot = XamlRoot };
            await dialog.ShowAsync();

            if (dialog.Result is null && dialog.SaveError is null) return;

            if (dialog.SaveError is not null)
            {
                ToastHelper.Error(ToastBar, "Failed to add medicine", dialog.SaveError.Message);
                return;
            }

            await LoadFromDbAsync();
            ToastHelper.Success(ToastBar, "Medicine added",
                $"{dialog.Result!.Name} has been added successfully.");
        }

        // ── Restock Medicine ───────────────────────────────────────────────────────
        private async void RestockMedicine_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
            var record = _vm.FindItem(item.Tag?.ToString() ?? string.Empty);
            if (record is null) return;

            var dialog = new RestockMedicine(record, _controller) { XamlRoot = XamlRoot };
            await dialog.ShowAsync();

            if (!dialog.Confirmed && dialog.SaveError is null) return;

            if (dialog.SaveError is not null)
            {
                ToastHelper.Error(ToastBar, "Failed to restock", dialog.SaveError.Message);
                return;
            }

            await LoadFromDbAsync();
            ToastHelper.Success(ToastBar, "Restock successful",
                $"{record.MedicineName} stock has been updated.");
        }

        // ── Edit Medicine ──────────────────────────────────────────────────────────
        private async void EditMedicine_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
            var record = _vm.FindItem(item.Tag?.ToString() ?? string.Empty);
            if (record is null) return;

            // Fetch full model from DB to get the real DateTime for ExpiryDate
            if (!int.TryParse(record.MedicineId, out var id)) return;
            var medicine = await _controller.GetMedicineByIdAsync(id);
            if (medicine is null) return;

            var dialog = new AddEditMedicine(_controller) { XamlRoot = XamlRoot };
            dialog.LoadForEdit(medicine.Id, medicine.Name, medicine.Unit, medicine.ExpiryDate);
            await dialog.ShowAsync();

            if (dialog.Result is null && dialog.SaveError is null) return;

            if (dialog.SaveError is not null)
            {
                ToastHelper.Error(ToastBar, "Failed to update medicine", dialog.SaveError.Message);
                return;
            }

            await LoadFromDbAsync();
            ToastHelper.Success(ToastBar, "Medicine updated",
                $"{dialog.Result!.Name} has been updated successfully.");
        }

        // ── Delete Medicine ────────────────────────────────────────────────────────
        private async void DeleteMedicine_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
            var record = _vm.FindItem(item.Tag?.ToString() ?? string.Empty);
            if (record is null) return;

            var dialog = new DeleteMedicine(record, _controller) { XamlRoot = XamlRoot };
            await dialog.ShowAsync();

            if (!dialog.Confirmed) return;

            if (dialog.SaveError is not null)
            {
                ToastHelper.Error(ToastBar, "Failed to delete medicine", dialog.SaveError.Message);
                return;
            }

            await LoadFromDbAsync();
            ToastHelper.Success(ToastBar, "Medicine deleted",
                $"{record.MedicineName} has been permanently removed.");
        }
    }
}