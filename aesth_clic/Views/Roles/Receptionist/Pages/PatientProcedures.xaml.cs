using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace aesth_clic.Views.Roles.Receptionist.Pages
{
    // ── Model ──────────────────────────────────────────────────────────────────────
    public class PatientProcedureItem
    {
        public string ProcedureRecordId { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string AvatarColor { get; set; } = "#5B2D8E";
        public string ProcedureName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        /// <summary>Hex foreground color for the status label (no badge background).</summary>
        public string StatusBadgeText { get; set; } = string.Empty;
        public string DateScheduled { get; set; } = string.Empty;
        public string Cost { get; set; } = string.Empty;

        public Visibility HasDate => string.IsNullOrWhiteSpace(DateScheduled) ? Visibility.Collapsed : Visibility.Visible;
        public Visibility NoDate => string.IsNullOrWhiteSpace(DateScheduled) ? Visibility.Visible : Visibility.Collapsed;
    }

    // ── Page ───────────────────────────────────────────────────────────────────────
    public sealed partial class PatientProcedures : Page
    {
        private List<PatientProcedureItem> _allProcedures = new();

        public PatientProcedures()
        {
            InitializeComponent();
            LoadSampleData();
            Loaded += (_, _) => ApplyFilters();
        }

        // ── Sample data ────────────────────────────────────────────────────────────
        private void LoadSampleData()
        {
            _allProcedures = new List<PatientProcedureItem>
            {
                BuildItem("pr1",  "p1",  "Maria Santos",    "Female", "Hydra Facial",        "Completed", "Jan 10, 2025", "₱3,500"),
                BuildItem("pr2",  "p1",  "Maria Santos",    "Female", "Botox Injection",      "Scheduled", "Mar 05, 2025", "₱8,000"),
                BuildItem("pr3",  "p2",  "Jose Reyes",      "Male",   "Laser Hair Removal",   "Pending",   "",             "₱5,200"),
                BuildItem("pr4",  "p3",  "Ana Cruz",        "Female", "Chemical Peel",        "Completed", "Feb 14, 2025", "₱2,800"),
                BuildItem("pr5",  "p3",  "Ana Cruz",        "Female", "Body Contouring",      "Scheduled", "Apr 20, 2025", "₱12,000"),
                BuildItem("pr6",  "p4",  "Carlo Mendoza",   "Male",   "Acne Scar Treatment",  "Pending",   "",             "₱6,500"),
                BuildItem("pr7",  "p5",  "Liza Flores",     "Female", "Dermal Fillers",       "Scheduled", "Mar 28, 2025", "₱9,500"),
                BuildItem("pr8",  "p6",  "Ramon Garcia",    "Male",   "Microdermabrasion",    "Completed", "Dec 22, 2024", "₱2,200"),
                BuildItem("pr9",  "p7",  "Sofia Aquino",    "Female", "Skin Brightening",     "Pending",   "",             "₱3,900"),
                BuildItem("pr10", "p8",  "Mark Villanueva", "Male",   "Laser Toning",         "Cancelled", "",             "₱4,800"),
                BuildItem("pr11", "p9",  "Grace Tan",       "Female", "Lip Augmentation",     "Scheduled", "May 02, 2025", "₱7,200"),
                BuildItem("pr12", "p10", "Kevin Lim",       "Male",   "Back Massage Therapy", "Completed", "Jan 30, 2025", "₱1,800"),
            };
        }

        private static PatientProcedureItem BuildItem(
            string recordId, string patientId, string patientName, string gender,
            string procedureName, string status, string dateScheduled, string cost)
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

            // No background badge — only a foreground color for the status text
            var statusFg = status switch
            {
                "Completed" => "#2E7D32",
                "Scheduled" => "#0078D4",
                "Pending" => "#F59E0B",
                "Cancelled" => "#C0392B",
                _ => "#666666"
            };

            return new PatientProcedureItem
            {
                ProcedureRecordId = recordId,
                PatientId = patientId,
                PatientName = patientName,
                Initials = initials.ToUpper(),
                AvatarColor = avatarColor,
                ProcedureName = procedureName,
                Status = status,
                StatusBadgeText = statusFg,   // foreground only
                DateScheduled = dateScheduled,
                Cost = cost,
            };
        }

        // ── Filtering ──────────────────────────────────────────────────────────────
        private void ApplyFilters()
        {
            if (ProcedureListControl is null) return;

            var search = SearchBox?.Text?.Trim().ToLower() ?? string.Empty;
            var statusTag = (StatusFilter?.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All";

            var filtered = _allProcedures.Where(p =>
            {
                bool matchSearch = string.IsNullOrEmpty(search)
                    || p.PatientName.ToLower().Contains(search)
                    || p.ProcedureName.ToLower().Contains(search);
                bool matchStatus = statusTag == "All" || p.Status == statusTag;
                return matchSearch && matchStatus;
            }).ToList();

            ProcedureListControl.ItemsSource = filtered;

            if (TxtTotalProcedures is not null) TxtTotalProcedures.Text = _allProcedures.Count.ToString();
            if (TxtPendingProcedures is not null) TxtPendingProcedures.Text = _allProcedures.Count(p => p.Status == "Pending").ToString();
            if (TxtScheduledProcedures is not null) TxtScheduledProcedures.Text = _allProcedures.Count(p => p.Status == "Scheduled").ToString();
            if (TxtCompletedProcedures is not null) TxtCompletedProcedures.Text = _allProcedures.Count(p => p.Status == "Completed").ToString();
            if (TxtRowCount is not null) TxtRowCount.Text = $"Showing {filtered.Count} procedure{(filtered.Count == 1 ? "" : "s")}";
        }

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
            => ApplyFilters();

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => ApplyFilters();

        // ── Header button ──────────────────────────────────────────────────────────
        private async void AddProcedureButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Add Procedure",
                Content = "Add Procedure dialog goes here.",
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Save",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = XamlRoot
            };
            await dialog.ShowAsync();
        }

        // ── Kebab menu ─────────────────────────────────────────────────────────────
        private void KebabMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;

            var recordId = btn.Tag?.ToString();
            var record = _allProcedures.FirstOrDefault(p => p.ProcedureRecordId == recordId);
            if (record is null) return;

            var menu = new MenuFlyout();

            // Schedule
            var scheduleItem = new MenuFlyoutItem
            {
                Text = "Schedule Appointment",
                Icon = new FontIcon { Glyph = "\uE787" }
            };
            scheduleItem.Click += async (_, _) =>
            {
                var dialog = new ContentDialog
                {
                    Title = $"Schedule — {record.ProcedureName}",
                    Content = $"Schedule appointment for {record.PatientName}'s {record.ProcedureName}.\nFunctionality will be implemented later.",
                    CloseButtonText = "Close",
                    XamlRoot = XamlRoot
                };
                await dialog.ShowAsync();
            };

            // View
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
                    Content = $"Patient: {record.PatientName}\nStatus: {record.Status}\nCost: {record.Cost}\nScheduled: {(string.IsNullOrWhiteSpace(record.DateScheduled) ? "Not yet scheduled" : record.DateScheduled)}",
                    CloseButtonText = "Close",
                    XamlRoot = XamlRoot
                };
                await dialog.ShowAsync();
            };

            // Edit
            var editItem = new MenuFlyoutItem
            {
                Text = "Edit Procedure",
                Icon = new FontIcon { Glyph = "\uE70F" }
            };
            editItem.Click += async (_, _) =>
            {
                var dialog = new ContentDialog
                {
                    Title = $"Edit — {record.ProcedureName}",
                    Content = "Edit Procedure dialog goes here.",
                    CloseButtonText = "Cancel",
                    PrimaryButtonText = "Save",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = XamlRoot
                };
                await dialog.ShowAsync();
            };

            // Separator + Cancel Procedure
            var separator = new MenuFlyoutSeparator();
            var cancelItem = new MenuFlyoutItem
            {
                Text = "Cancel Procedure",
                Icon = new FontIcon { Glyph = "\uE711" },
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 192, 57, 43))
            };
            cancelItem.Click += async (_, _) =>
            {
                var dialog = new ContentDialog
                {
                    Title = "Cancel Procedure?",
                    Content = $"Are you sure you want to cancel {record.PatientName}'s {record.ProcedureName}?",
                    PrimaryButtonText = "Yes, Cancel",
                    CloseButtonText = "Keep",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = XamlRoot
                };
                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    record.Status = "Cancelled";
                    ApplyFilters();
                }
            };

            menu.Items.Add(scheduleItem);
            menu.Items.Add(viewItem);
            menu.Items.Add(editItem);
            menu.Items.Add(separator);
            menu.Items.Add(cancelItem);

            menu.ShowAt(btn);
        }
    }
}