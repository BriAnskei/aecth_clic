using aesth_clic.Views.Roles.Receptionist.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace aesth_clic.ViewModels.Receptionist
{
    internal class DoctorsAvailabilityViewModel : INotifyPropertyChanged
    {
        private readonly List<DoctorAvailabilityItem> _allDoctors = new();
        public ObservableCollection<DoctorAvailabilityItem> DisplayedDoctors { get; } = new();

        // ──────────────────────────────────────────────────────
        // FILTER STATE
        // ──────────────────────────────────────────────────────
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ApplyFilters(); }
        }

        private string _selectedStatus = "All";
        public string SelectedStatus
        {
            get => _selectedStatus;
            set { _selectedStatus = value; OnPropertyChanged(); ApplyFilters(); }
        }

        // ──────────────────────────────────────────────────────
        // KPI COUNTERS  (reflect DisplayedDoctors — i.e. filtered)
        // ──────────────────────────────────────────────────────
        public int TotalDoctors => DisplayedDoctors.Count;
        public int AvailableDoctors => DisplayedDoctors.Count(d => d.Status == "Available");
        public int BusyDoctors => DisplayedDoctors.Count(d => d.Status == "Busy");

        // ──────────────────────────────────────────────────────
        // LOAD FROM DB  (maps DTO list → DoctorAvailabilityItem list)
        // ──────────────────────────────────────────────────────
        public void LoadFromDb(IEnumerable<(
            string DoctorId,
            string FullName,
            string AvailabilityStatus)> doctors)
        {
            _allDoctors.Clear();

            foreach (var d in doctors)
                _allDoctors.Add(BuildItem(d.DoctorId, d.FullName, d.AvailabilityStatus));

            ApplyFilters();
        }

        // ──────────────────────────────────────────────────────
        // FACTORY HELPER
        // ──────────────────────────────────────────────────────
        public static DoctorAvailabilityItem BuildItem(string id, string name, string rawStatus)
        {
            // Normalize DTO lowercase values ("available" / "busy") → display casing
            var status = rawStatus?.Trim().ToLower() switch
            {
                "available" => "Available",
                "busy" => "Busy",
                _ => rawStatus ?? string.Empty
            };

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

        // ──────────────────────────────────────────────────────
        // FILTERS
        // ──────────────────────────────────────────────────────
        public void ApplyFilters()
        {
            var filtered = _allDoctors.Where(d =>
                (string.IsNullOrEmpty(SearchText)
                    || d.DoctorName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                && (SelectedStatus == "All" || d.Status == SelectedStatus)
            ).OrderBy(d => d.DoctorName);

            DisplayedDoctors.Clear();
            foreach (var d in filtered)
                DisplayedDoctors.Add(d);

            // Notify KPIs so code-behind can refresh text
            OnPropertyChanged(nameof(TotalDoctors));
            OnPropertyChanged(nameof(AvailableDoctors));
            OnPropertyChanged(nameof(BusyDoctors));
        }

        // ──────────────────────────────────────────────────────
        // INPC
        // ──────────────────────────────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}