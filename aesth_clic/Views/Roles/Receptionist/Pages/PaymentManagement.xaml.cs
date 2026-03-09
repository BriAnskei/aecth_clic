using aesth_clic.Tenant.Controller;
using aesth_clic.Utils;
using aesth_clic.ViewModels.Receptionist;
using aesth_clic.Views.Roles.Receptionist.Modals;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;
using Windows.UI;

namespace aesth_clic.Views.Roles.Receptionist.Pages
{
    // ── Page ───────────────────────────────────────────────────────────────────
    public sealed partial class PaymentManagement : Page
    {
        private readonly PaymentManagementViewModel _vm = new();
        private readonly ProcedurePaymentController _paymentController;

        public PaymentManagement()
        {
            InitializeComponent();

            _paymentController = App.Services.GetRequiredService<ProcedurePaymentController>();

            PaymentListControl.ItemsSource = _vm.DisplayedPayments;

            _ = LoadFromDbAsync();
        }

        // ── Data loading ───────────────────────────────────────────────────────
        private async System.Threading.Tasks.Task LoadFromDbAsync()
        {
            try
            {
                var payments = await _paymentController.GetAllPaymentsAsync();

                _vm.LoadFromDb(payments);

                PaymentListControl.ItemsSource = null;
                PaymentListControl.ItemsSource = _vm.DisplayedPayments;

                UpdateRowCount();
            }
            catch (Exception ex)
            {
                ToastHelper.Error(ToastBar, "Failed to load payments", ex.Message);
            }
        }

        // ── Row count footer ───────────────────────────────────────────────────
        private void UpdateRowCount()
        {
            if (TxtRowCount is null) return;
            var count = _vm.DisplayedPayments.Count;
            TxtRowCount.Text = $"Showing {count} payment{(count != 1 ? "s" : "")}";
        }

        // ── Search + Filter ────────────────────────────────────────────────────
        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            _vm.SearchText = sender.Text ?? string.Empty;
            UpdateRowCount();
        }

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _vm.SelectedStatus =
                (StatusFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All";
            UpdateRowCount();
        }

        // ── Kebab menu ─────────────────────────────────────────────────────────
        private void KebabMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;

            var paymentId = btn.Tag?.ToString();
            var record = _vm.FindPayment(paymentId ?? string.Empty);
            if (record is null) return;

            var menu = new MenuFlyout();

            // ── "Mark as Paid" — Pending only ─────────────────────────────────
            if (record.Status == "Pending")
            {
                var markPaidItem = new MenuFlyoutItem
                {
                    Text = "Mark as Paid",
                    Icon = new FontIcon { Glyph = "\uE8FB" }   // checkmark-circle icon
                };
                markPaidItem.Click += async (_, _) =>
                {
                    // Confirmation dialog showing procedure name + price
                    var confirmContent = new StackPanel { Spacing = 12, MinWidth = 300 };

                    // Summary pill
                    var summaryBorder = new Border
                    {
                        Background = new SolidColorBrush(Color.FromArgb(255, 237, 228, 249)),
                        CornerRadius = new CornerRadius(8),
                        Padding = new Thickness(14, 10, 14, 10),
                    };
                    var summaryStack = new StackPanel { Spacing = 4 };
                    summaryStack.Children.Add(new TextBlock
                    {
                        Text = record.ProcedureName,
                        FontSize = 14,
                        FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                        Foreground = new SolidColorBrush(Color.FromArgb(255, 45, 21, 84)),
                    });
                    summaryStack.Children.Add(new TextBlock
                    {
                        Text = record.Amount,
                        FontSize = 20,
                        FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                        Foreground = new SolidColorBrush(Color.FromArgb(255, 91, 45, 142)),
                    });
                    summaryBorder.Child = summaryStack;
                    confirmContent.Children.Add(summaryBorder);

                    confirmContent.Children.Add(new TextBlock
                    {
                        Text = $"Mark this payment as completed for {record.PatientName}?",
                        FontSize = 13,
                        Foreground = new SolidColorBrush(Color.FromArgb(255, 45, 21, 84)),
                        TextWrapping = TextWrapping.Wrap,
                    });

                    var confirmDialog = new ContentDialog
                    {
                        Title = "Mark as Paid",
                        Content = confirmContent,
                        PrimaryButtonText = "Confirm Payment",
                        CloseButtonText = "Cancel",
                        DefaultButton = ContentDialogButton.Primary,
                        PrimaryButtonStyle = (Style)Application.Current.Resources["PurplePrimaryButtonStyle"],
                        XamlRoot = XamlRoot,
                    };

                    var result = await confirmDialog.ShowAsync();

                    if (result != ContentDialogResult.Primary) return;

                    try
                    {
                        if (!int.TryParse(record.PaymentId, out int pid))
                            throw new InvalidOperationException("Invalid payment ID.");

                        await _paymentController.MarkPaymentCompletedAsync(pid);

                        ToastHelper.Success(ToastBar,
                            "Payment Completed",
                            $"{record.ProcedureName} for {record.PatientName} marked as paid.");

                        // Reload full list from DB
                        await LoadFromDbAsync();
                    }
                    catch (Exception ex)
                    {
                        ToastHelper.Error(ToastBar, "Failed to mark payment", ex.Message);
                    }
                };

                menu.Items.Add(markPaidItem);
                menu.Items.Add(new MenuFlyoutSeparator());
            }

            // ── "View Details" — all statuses ─────────────────────────────────
            var viewItem = new MenuFlyoutItem
            {
                Text = "View Details",
                Icon = new FontIcon { Glyph = "\uE7B3" }
            };
            viewItem.Click += async (_, _) =>
            {
                var detailDialog = new ViewPaymentDetails(record)
                {
                    XamlRoot = XamlRoot,
                };
                await detailDialog.ShowAsync();
            };
            menu.Items.Add(viewItem);

            menu.ShowAt(btn);
        }
    }
}