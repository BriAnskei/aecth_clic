using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace aesth_clic.Views.Roles.Doctor.Pages
{
    public class ProcedureItem
    {
        public string ProcedureItemId { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string AvatarColor { get; set; } = "#5B2D8E";
        public string ProcedureName { get; set; } = string.Empty;
        public string AppointmentDate { get; set; } = string.Empty;
        public string AppointmentTime { get; set; } = string.Empty;
    }

    public sealed partial class ProcedureManagement : Page
    {
        // Master list — only appointments that have a date set are included here.
        // When connected to a real DB, filter by: appointment date IS NOT NULL.
        private List<ProcedureItem> _allProcedures = new();

        public ProcedureManagement()
        {
            InitializeComponent();
            LoadData();
            Loaded += (_, _) => ApplyFilters();
        }

        // ---------------------------------------------------------------------------
        // Data
        // ---------------------------------------------------------------------------

        private void LoadData()
        {
            // Mirrors the AppointmentManagement sample data.
            // Only rows that have a scheduled date are included (all 12 qualify here).
            // When you wire up a real DB, replace this with a query such as:
            //   SELECT * FROM Appointments WHERE AppointmentDate IS NOT NULL
            _allProcedures = new List<ProcedureItem>
            {
                Build("a1",  "p1",  "Maria Santos",    "Female", "Botox Injection",      "Mar 05, 2025", "10:00 AM"),
                Build("a2",  "p3",  "Ana Cruz",        "Female", "Body Contouring",      "Apr 20, 2025", "02:30 PM"),
                Build("a3",  "p5",  "Liza Flores",     "Female", "Dermal Fillers",       "Mar 28, 2025", "11:00 AM"),
                Build("a4",  "p9",  "Grace Tan",       "Female", "Lip Augmentation",     "May 02, 2025", "03:00 PM"),
                Build("a5",  "p1",  "Maria Santos",    "Female", "Hydra Facial",         "Jan 10, 2025", "09:00 AM"),
                Build("a6",  "p3",  "Ana Cruz",        "Female", "Chemical Peel",        "Feb 14, 2025", "01:00 PM"),
                Build("a7",  "p6",  "Ramon Garcia",    "Male",   "Microdermabrasion",    "Dec 22, 2024", "10:30 AM"),
                Build("a8",  "p10", "Kevin Lim",       "Male",   "Back Massage Therapy", "Jan 30, 2025", "04:00 PM"),
                Build("a9",  "p2",  "Jose Reyes",      "Male",   "Laser Hair Removal",   "Jun 10, 2025", "09:30 AM"),
                Build("a10", "p4",  "Carlo Mendoza",   "Male",   "Acne Scar Treatment",  "Jun 15, 2025", "11:30 AM"),
                Build("a11", "p7",  "Sofia Aquino",    "Female", "Skin Brightening",     "Jun 20, 2025", "02:00 PM"),
                Build("a12", "p8",  "Mark Villanueva", "Male",   "Laser Toning",         "Jul 01, 2025", "03:30 PM"),
            };
        }

        private static ProcedureItem Build(
            string id, string patientId, string patientName, string gender,
            string procedureName, string date, string time)
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

            return new ProcedureItem
            {
                ProcedureItemId = id,
                PatientId = patientId,
                PatientName = patientName,
                Initials = initials.ToUpper(),
                AvatarColor = avatarColor,
                ProcedureName = procedureName,
                AppointmentDate = date,
                AppointmentTime = time,
            };
        }

        // ---------------------------------------------------------------------------
        // Filtering
        // ---------------------------------------------------------------------------

        private void ApplyFilters()
        {
            if (ProcedureListControl is null) return;

            var search = SearchBox?.Text?.Trim().ToLower() ?? string.Empty;

            var filtered = _allProcedures.Where(p =>
                string.IsNullOrEmpty(search)
                || p.PatientName.ToLower().Contains(search)
                || p.ProcedureName.ToLower().Contains(search)
            ).ToList();

            ProcedureListControl.ItemsSource = filtered;

            if (TxtRowCount is not null)
                TxtRowCount.Text = $"Showing {filtered.Count} procedure{(filtered.Count == 1 ? "" : "s")}";
        }

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
            => ApplyFilters();

        // ---------------------------------------------------------------------------
        // Actions
        // ---------------------------------------------------------------------------

        private async void MarkDone_Click(object sender, RoutedEventArgs e)
        {
            var id = (sender as Button)?.Tag?.ToString();
            var record = _allProcedures.FirstOrDefault(p => p.ProcedureItemId == id);
            if (record is null) return;

            var dialog = new ContentDialog
            {
                Title = "Mark as Done",
                Content = $"Mark \"{record.ProcedureName}\" for {record.PatientName} as completed?\nThis will remove it from your procedure list.",
                PrimaryButtonText = "Mark Done",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                _allProcedures.Remove(record);
                ApplyFilters();
            }
        }
    }
}