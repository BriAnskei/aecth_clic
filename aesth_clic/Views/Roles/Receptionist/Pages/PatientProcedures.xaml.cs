using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace aesth_clic.Views.Roles.Receptionist.Pages
{
    // ── Data Model ─────────────────────────────────────────────────────────────
    public class PatientProcedureItem
    {
        public string ProcedureRecordId { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string AvatarColor { get; set; } = "#5B2D8E";
        public string ProcedureName { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public string StatusBadgeText { get; set; } = "#F59E0B";
        public string DateScheduled { get; set; } = string.Empty;
        public string Cost { get; set; } = string.Empty;

        // Visibility helpers for the "Date Scheduled" column
        public Microsoft.UI.Xaml.Visibility HasDate
            => string.IsNullOrEmpty(DateScheduled)
               ? Microsoft.UI.Xaml.Visibility.Collapsed
               : Microsoft.UI.Xaml.Visibility.Visible;

        public Microsoft.UI.Xaml.Visibility NoDate
            => string.IsNullOrEmpty(DateScheduled)
               ? Microsoft.UI.Xaml.Visibility.Visible
               : Microsoft.UI.Xaml.Visibility.Collapsed;
    }

    // ── Page ───────────────────────────────────────────────────────────────────
    public sealed partial class PatientProcedures : Page
    {
        private List<PatientProcedureItem> _allProcedures = new();

        public PatientProcedures()
        {
            InitializeComponent();
            LoadSampleData();
            Loaded += (_, _) => ApplyFilters();
        }

        // ── Sample Data ────────────────────────────────────────────────────────
        private void LoadSampleData()
        {
            _allProcedures = new List<PatientProcedureItem>
            {
                Build("pr1",  "p1",  "Maria Santos",    "Female", "Botox Injection",      "Completed",  "Mar 05, 2025", "₱3,500"),
                Build("pr2",  "p3",  "Ana Cruz",        "Female", "Body Contouring",      "Scheduled",  "Apr 20, 2025", "₱8,000"),
                Build("pr3",  "p5",  "Liza Flores",     "Female", "Dermal Fillers",       "Scheduled",  "Mar 28, 2025", "₱5,000"),
                Build("pr4",  "p9",  "Grace Tan",       "Female", "Lip Augmentation",     "Pending",    "",             "₱4,500"),
                Build("pr5",  "p1",  "Maria Santos",    "Female", "Hydra Facial",         "Completed",  "Jan 10, 2025", "₱2,000"),
                Build("pr6",  "p3",  "Ana Cruz",        "Female", "Chemical Peel",        "Completed",  "Feb 14, 2025", "₱2,500"),
                Build("pr7",  "p6",  "Ramon Garcia",    "Male",   "Microdermabrasion",    "Cancelled",  "Dec 22, 2024", "₱1,800"),
                Build("pr8",  "p10", "Kevin Lim",       "Male",   "Back Massage Therapy", "Completed",  "Jan 30, 2025", "₱1,200"),
                Build("pr9",  "p2",  "Jose Reyes",      "Male",   "Laser Hair Removal",   "Scheduled",  "Jun 10, 2025", "₱6,000"),
                Build("pr10", "p4",  "Carlo Mendoza",   "Male",   "Acne Scar Treatment",  "Pending",    "",             "₱4,000"),
                Build("pr11", "p7",  "Sofia Aquino",    "Female", "Skin Brightening",     "Scheduled",  "Jun 20, 2025", "₱3,200"),
                Build("pr12", "p8",  "Mark Villanueva", "Male",   "Laser Toning",         "Pending",    "",             "₱5,500"),
            };
        }

        private static PatientProcedureItem Build(
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

            var statusColor = status switch
            {
                "Completed" => "#2E7D32",
                "Scheduled" => "#0078D4",
                "Cancelled" => "#C0392B",
                _ => "#F59E0B"   // Pending
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
                StatusBadgeText = statusColor,
                DateScheduled = dateScheduled,
                Cost = cost,
            };
        }

        // ── Filtering ──────────────────────────────────────────────────────────
        private void ApplyFilters()
        {
            if (ProcedureListControl is null) return;

            var search = SearchBox?.Text?.Trim().ToLower() ?? string.Empty;
            var statusTag = (StatusFilter?.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All";

            var filtered = _allProcedures.Where(p =>
                (string.IsNullOrEmpty(search)
                 || p.PatientName.ToLower().Contains(search)
                 || p.ProcedureName.ToLower().Contains(search))
                && (statusTag == "All" || p.Status == statusTag)
            ).ToList();

            ProcedureListControl.ItemsSource = filtered;

            var total = _allProcedures.Count;
            var pending = _allProcedures.Count(p => p.Status == "Pending");
            var scheduled = _allProcedures.Count(p => p.Status == "Scheduled");
            var completed = _allProcedures.Count(p => p.Status == "Completed");

            if (TxtTotalProcedures is not null) TxtTotalProcedures.Text = total.ToString();
            if (TxtPendingProcedures is not null) TxtPendingProcedures.Text = pending.ToString();
            if (TxtScheduledProcedures is not null) TxtScheduledProcedures.Text = scheduled.ToString();
            if (TxtCompletedProcedures is not null) TxtCompletedProcedures.Text = completed.ToString();
            if (TxtRowCount is not null)
                TxtRowCount.Text = $"Showing {filtered.Count} procedure{(filtered.Count == 1 ? "" : "s")}";
        }

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
            => ApplyFilters();

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => ApplyFilters();

        private void KebabMenu_Click(object sender, RoutedEventArgs e) { }

        // ── Add Procedure ──────────────────────────────────────────────────────
        private async void AddProcedureButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Add Procedure",
                Content = "Add procedure form will be implemented here.",
                CloseButtonText = "Close",
                XamlRoot = XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}