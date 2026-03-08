using aesth_clic.Views.Roles.Receptionist.Pages;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace aesth_clic.ViewModels.Receptionist
{
    internal class AppointmentManagementViewModel : INotifyPropertyChanged
    {
        private readonly List<AppointmentItem> _allAppointments = new();
        public ObservableCollection<AppointmentItem> DisplayedAppointments { get; } = new();

        // ── Displayed row count (filtered) ─────────────────────────────────────
        public int DisplayedCount => DisplayedAppointments.Count;

        // ── Filter state ───────────────────────────────────────────────────────
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ApplyFilters(); }
        }

        // ── Load ───────────────────────────────────────────────────────────────
        public void LoadFromDb(IEnumerable<AppointmentItem> items)
        {
            _allAppointments.Clear();
            foreach (var item in items)
                _allAppointments.Add(item);

            ApplyFilters();
        }

        // ── Filters ────────────────────────────────────────────────────────────
        public void ApplyFilters()
        {
            var search = SearchText.Trim().ToLower();

            var filtered = _allAppointments.Where(a =>
                string.IsNullOrEmpty(search)
                || a.PatientName.ToLower().Contains(search)
                || a.ProcedureName.ToLower().Contains(search)
            );

            DisplayedAppointments.Clear();
            foreach (var a in filtered)
                DisplayedAppointments.Add(a);

            OnPropertyChanged(nameof(DisplayedCount));
        }

        // ── Helpers ────────────────────────────────────────────────────────────
        public AppointmentItem? FindAppointment(string appointmentId) =>
            _allAppointments.FirstOrDefault(a => a.AppointmentId == appointmentId);

        // ── INPC ───────────────────────────────────────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}