using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace aesth_clic.Views.Roles.Receptionist.Pages
{
    // ── Model ──────────────────────────────────────────────────────────────────────
    public class PaymentItem
    {
        public string PaymentId { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string AvatarColor { get; set; } = "#5B2D8E";
        public string ProcedureName { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        /// <summary>Hex foreground color for the status label (no badge background).</summary>
        public string StatusColor { get; set; } = string.Empty;
    }

    // ── Page ───────────────────────────────────────────────────────────────────────
    public sealed partial class PaymentManagement : Page
    {
        private List<PaymentItem> _allPayments = new();

        public PaymentManagement()
        {
            InitializeComponent();
            LoadSampleData();
            Loaded += (_, _) => ApplyFilters();
        }

        // ── Sample data ────────────────────────────────────────────────────────────
        private void LoadSampleData()
        {
            _allPayments = new List<PaymentItem>
            {
                BuildItem("pay1",  "p1",  "Maria Santos",    "Female", "Hydra Facial",        "Completed", "₱3,500"),
                BuildItem("pay2",  "p1",  "Maria Santos",    "Female", "Botox Injection",      "Pending",   "₱8,000"),
                BuildItem("pay3",  "p2",  "Jose Reyes",      "Male",   "Laser Hair Removal",   "Pending",   "₱5,200"),
                BuildItem("pay4",  "p3",  "Ana Cruz",        "Female", "Chemical Peel",        "Completed", "₱2,800"),
                BuildItem("pay5",  "p3",  "Ana Cruz",        "Female", "Body Contouring",      "Pending",   "₱12,000"),
                BuildItem("pay6",  "p4",  "Carlo Mendoza",   "Male",   "Acne Scar Treatment",  "Pending",   "₱6,500"),
                BuildItem("pay7",  "p5",  "Liza Flores",     "Female", "Dermal Fillers",       "Pending",   "₱9,500"),
                BuildItem("pay8",  "p6",  "Ramon Garcia",    "Male",   "Microdermabrasion",    "Completed", "₱2,200"),
                BuildItem("pay9",  "p7",  "Sofia Aquino",    "Female", "Skin Brightening",     "Pending",   "₱3,900"),
                BuildItem("pay10", "p8",  "Mark Villanueva", "Male",   "Laser Toning",         "Pending",   "₱4,800"),
                BuildItem("pay11", "p9",  "Grace Tan",       "Female", "Lip Augmentation",     "Pending",   "₱7,200"),
                BuildItem("pay12", "p10", "Kevin Lim",       "Male",   "Back Massage Therapy", "Completed", "₱1,800"),
            };
        }

        private static PaymentItem BuildItem(
            string paymentId, string patientId, string patientName, string gender,
            string procedureName, string status, string amount)
        {
            var parts = patientName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var initials = parts.Length >= 2
                ? $"{parts[0][0]}{parts[^1][0]}"
                : patientName.Length > 0 ? patientName[0].ToString() : "?";

            var avatarColor = gender switch
            {
                "Female" => "#C2185B",
                "Male" => "#0078D4",
                _ => "#5B2D8E"
            };

            var statusColor = status switch
            {
                "Completed" => "#2E7D32",
                "Pending" => "#F59E0B",
                _ => "#666666"
            };

            return new PaymentItem
            {
                PaymentId = paymentId,
                PatientId = patientId,
                PatientName = patientName,
                Initials = initials.ToUpper(),
                AvatarColor = avatarColor,
                ProcedureName = procedureName,
                Amount = amount,
                Status = status,
                StatusColor = statusColor,
            };
        }

        // ── Filtering ──────────────────────────────────────────────────────────────
        private void ApplyFilters()
        {
            if (PaymentListControl is null) return;

            var search = SearchBox?.Text?.Trim().ToLower() ?? string.Empty;
            var statusTag = (StatusFilter?.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All";

            var filtered = _allPayments.Where(p =>
            {
                bool matchSearch = string.IsNullOrEmpty(search)
                    || p.PatientName.ToLower().Contains(search)
                    || p.ProcedureName.ToLower().Contains(search);
                bool matchStatus = statusTag == "All" || p.Status == statusTag;
                return matchSearch && matchStatus;
            }).ToList();

            PaymentListControl.ItemsSource = filtered;

            if (TxtRowCount is not null)
                TxtRowCount.Text = $"Showing {filtered.Count} payment{(filtered.Count == 1 ? "" : "s")}";
        }

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
            => ApplyFilters();

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => ApplyFilters();

        // ── Kebab menu ─────────────────────────────────────────────────────────────
        private void KebabMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;

            var paymentId = btn.Tag?.ToString();
            var record = _allPayments.FirstOrDefault(p => p.PaymentId == paymentId);
            if (record is null) return;

            var menu = new MenuFlyout();

            if (record.Status == "Pending")
            {
                // Add Payment (only for Pending)
                var addPaymentItem = new MenuFlyoutItem
                {
                    Text = "Add Payment",
                    Icon = new FontIcon { Glyph = "\uE8C7" }
                };
                addPaymentItem.Click += async (_, _) =>
                {
                    var dialog = new ContentDialog
                    {
                        Title = $"Add Payment — {record.ProcedureName}",
                        Content = "Add Payment dialog goes here.",
                        CloseButtonText = "Cancel",
                        PrimaryButtonText = "Confirm",
                        DefaultButton = ContentDialogButton.Primary,
                        XamlRoot = XamlRoot
                    };
                    await dialog.ShowAsync();
                };
                menu.Items.Add(addPaymentItem);
                menu.Items.Add(new MenuFlyoutSeparator());
            }

            // View Details (available for all statuses)
            var viewItem = new MenuFlyoutItem
            {
                Text = "View Details",
                Icon = new FontIcon { Glyph = "\uE7B3" }
            };
            viewItem.Click += async (_, _) =>
            {
                var dialog = new ContentDialog
                {
                    Title = record.ProcedureName,
                    Content = $"Patient: {record.PatientName}\nProcedure: {record.ProcedureName}\nAmount: {record.Amount}\nStatus: {record.Status}",
                    CloseButtonText = "Close",
                    XamlRoot = XamlRoot
                };
                await dialog.ShowAsync();
            };
            menu.Items.Add(viewItem);

            menu.ShowAt(btn);
        }
    }
}