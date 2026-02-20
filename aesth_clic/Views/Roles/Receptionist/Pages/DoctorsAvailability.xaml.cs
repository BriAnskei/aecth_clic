using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace aesth_clic.Views.Roles.Receptionist.Pages
{
    // ── Data Model ─────────────────────────────────────────────────────────────
    public class DoctorAvailabilityItem
    {
        public string DoctorId { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;

        // "Available" or "Busy"
        public string Status { get; set; } = string.Empty;

        // Derived display properties
        public string StatusBackground { get; set; } = string.Empty;
        public string StatusForeground { get; set; } = string.Empty;
        public Windows.UI.Color StatusDotColor { get; set; }
    }

    // ── Page ───────────────────────────────────────────────────────────────────
    public sealed partial class DoctorsAvailability : Page
    {
        private List<DoctorAvailabilityItem> _allDoctors = new();
        private string _activeFilter = "All"; // "All" | "Available" | "Busy"

        public DoctorsAvailability()
        {
            InitializeComponent();
            LoadSampleData();
            Loaded += (_, _) => ApplyFilters();
        }

        // ── Sample Data ────────────────────────────────────────────────────────
        private void LoadSampleData()
        {
            _allDoctors = new List<DoctorAvailabilityItem>
            {
                Build("d1", "Dr. Andrea Reyes",     "Available"),
                Build("d2", "Dr. Marco Santos",     "Busy"),
                Build("d3", "Dr. Camille Torres",   "Available"),
                Build("d4", "Dr. Luis Gonzales",    "Busy"),
                Build("d5", "Dr. Patricia Lim",     "Available"),
                Build("d6", "Dr. Ramon Aquino",     "Busy"),
                Build("d7", "Dr. Sofia Dela Cruz",  "Available"),
                Build("d8", "Dr. James Villanueva", "Available"),
            };
        }

        private static DoctorAvailabilityItem Build(string id, string name, string status)
        {
            bool isAvailable = status == "Available";

            return new DoctorAvailabilityItem
            {
                DoctorId = id,
                DoctorName = name,
                Status = status,
                StatusBackground = isAvailable ? "#F0FAF0" : "#FDECEA",
                StatusForeground = isAvailable ? "#2E7D32" : "#C0392B",
                StatusDotColor = isAvailable
                    ? Windows.UI.Color.FromArgb(255, 46, 125, 50)
                    : Windows.UI.Color.FromArgb(255, 192, 57, 43),
            };
        }

        // ── Filtering ──────────────────────────────────────────────────────────
        private void ApplyFilters()
        {
            if (DoctorListControl is null) return;

            var search = SearchBox?.Text?.Trim().ToLower() ?? string.Empty;

            var filtered = _allDoctors.Where(d =>
            {
                bool matchesSearch = string.IsNullOrEmpty(search)
                    || d.DoctorName.ToLower().Contains(search);

                bool matchesFilter = _activeFilter == "All"
                    || d.Status == _activeFilter;

                return matchesSearch && matchesFilter;
            }).ToList();

            DoctorListControl.ItemsSource = filtered;

            if (TxtRowCount is not null)
                TxtRowCount.Text = $"Showing {filtered.Count} doctor{(filtered.Count == 1 ? "" : "s")}";
        }

        // ── Event Handlers ─────────────────────────────────────────────────────
        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
            => ApplyFilters();

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _activeFilter = (StatusFilter.SelectedIndex) switch
            {
                1 => "Available",
                2 => "Busy",
                _ => "All"
            };
            ApplyFilters();
        }
    }
}