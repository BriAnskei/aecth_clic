using aesth_clic.Master.Controller;
using aesth_clic.Utils;
using aesth_clic.ViewModels.SuperAdmin;
using aesth_clic.Views.Roles.SuperAdmin.Modals;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;

namespace aesth_clic.Views.Roles.SuperAdmin.Pages
{
    public sealed partial class PaymentManagement : Page
    {
        private readonly PaymentManagementViewModel _vm = new();
        private readonly SubscriptionController _paymentController;

        public PaymentManagement()
        {
            InitializeComponent();

            _paymentController = App.Services.GetRequiredService<SubscriptionController>();

            PaymentListControl.ItemsSource = _vm.DisplayedSubscriptions;

            _ = LoadFromDbAsync();
        }

        // ──────────────────────────────────────────────────────────────────────
        // LOADING STATE
        // ──────────────────────────────────────────────────────────────────────
        private void UpdateLoadingState(bool isLoading)
        {
            KpiGrid.IsHitTestVisible = !isLoading;
            KpiGrid.Opacity = isLoading ? 0.4 : 1.0;
            FilterToolbar.IsHitTestVisible = !isLoading;
            FilterToolbar.Opacity = isLoading ? 0.4 : 1.0;
            SkeletonTable.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
            RealTable.Visibility = isLoading ? Visibility.Collapsed : Visibility.Visible;
        }

        // ──────────────────────────────────────────────────────────────────────
        // DATA LOADING
        // ──────────────────────────────────────────────────────────────────────
        private async System.Threading.Tasks.Task LoadFromDbAsync()
        {
            _vm.IsLoading = true;
            UpdateLoadingState(true);

            try
            {
                var rows = await _paymentController.GetAllSubcriptionsAsync();
                _vm.LoadFromDb(rows);
                UpdateKpiCards();
            }
            catch (Exception ex)
            {
                ToastHelper.Error(ToastBar, "Failed to load payments", ex.Message);
            }
            finally
            {
                _vm.IsLoading = false;
                UpdateLoadingState(false);
            }
        }

        // ──────────────────────────────────────────────────────────────────────
        // KPI CARDS
        // ──────────────────────────────────────────────────────────────────────
        private void UpdateKpiCards()
        {
            TxtMrr.Text = _vm.MrrDisplay;
            TxtCollected.Text = _vm.CollectedThisMonthDisplay;
            TxtOverdue.Text = _vm.OverdueAmountDisplay;
            TxtSubscriberCount.Text = _vm.SubscriberCountDisplay;
            TxtCollectionRate.Text = _vm.CollectionRateDisplay;
            TxtOverdueCount.Text = _vm.OverdueCountDisplay;
            TxtRowCount.Text = _vm.RowCountDisplay;
            TxtFooterCount.Text = $"Showing {_vm.DisplayedSubscriptions.Count} invoice{(_vm.DisplayedSubscriptions.Count != 1 ? "s" : "")}";
        }

        // ──────────────────────────────────────────────────────────────────────
        // SEARCH + FILTERS
        // ──────────────────────────────────────────────────────────────────────
        private void SearchBox_TextChanged(
            AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            _vm.SearchText = sender.Text;
            UpdateKpiCards();
        }

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _vm.SelectedStatus =
                (StatusFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All";
            UpdateKpiCards();
        }

        private void TierFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _vm.SelectedTier =
                (TierFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All";
            UpdateKpiCards();
        }

        // ──────────────────────────────────────────────────────────────────────
        // MARK AS PAID
        // ──────────────────────────────────────────────────────────────────────
        private async void MarkAsPaid_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
            if (item.DataContext is not SubscriptionItem sub) return;



            var dialog = new ConfirmMarkAsPaid(
                clientName: sub.FullName,
                clinicName: sub.ClinicName,
                amount: sub.MonthlyAmountDisplay,
                dueDate: sub.EndDateDisplay)
            {
                XamlRoot = XamlRoot
            };

            await dialog.ShowAsync();
            if (!dialog.Confirmed) return;

            try
            {
                await _paymentController.MarkCurrentMonthAsPaidAsync(sub.SubscriptionId);

                // Reload from DB so dates reflect the actual persisted state
                await LoadFromDbAsync();

                ToastHelper.Success(ToastBar,
                    "Payment recorded",
                    $"{sub.FullName} — {sub.ClinicName} marked as paid.");
            }
            catch (Exception ex)
            {
                ToastHelper.Error(ToastBar, "Failed to record payment", ex.Message);
            }
        }

        // ──────────────────────────────────────────────────────────────────────
        // GENERATE RECEIPT
        // ──────────────────────────────────────────────────────────────────────
        private async void GenerateReceipt_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
            if (item.DataContext is not SubscriptionItem sub) return;

            var receiptData = new ReceiptData
            {
                ReceiptNumber = ReceiptService.GenerateReceiptNumber(),
                PaymentDate = sub.StartDateDisplay,
                ClientName = sub.FullName,
                Email = sub.Email,
                ClinicName = sub.ClinicName,
                Tier = sub.Tier,
                Amount = sub.MonthlyAmount.ToString("PHP #,##0.00"),
                NextDueDate = sub.EndDateDisplay,
                IssuedBy = "SuperAdmin",
                Status = sub.DisplayStatus,
            };

            try
            {
                ToastHelper.Info(ToastBar, "Generating receipt…",
                    $"Preparing receipt for {sub.FullName}.");

                await ReceiptService.GenerateAndOpenAsync(receiptData);

                ToastHelper.Success(ToastBar, "Receipt opened",
                    $"{receiptData.ReceiptNumber} opened in your PDF viewer.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("=== RECEIPT GENERATION ERROR ===");
                Debug.WriteLine(ex.ToString());
                Debug.WriteLine("================================");
                ToastHelper.Error(ToastBar, "Failed to generate receipt", ex.Message);
            }
        }
    }
}