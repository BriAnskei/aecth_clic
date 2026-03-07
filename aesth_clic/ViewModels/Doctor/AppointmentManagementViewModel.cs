using aesth_clic.Views.Roles.Doctor.Pages;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.UI;

namespace aesth_clic.ViewModels.Doctor
{
    internal class AppointmentManagementViewModel : INotifyPropertyChanged
    {
        private readonly List<AppointmentItem> _allAppointments = new();
        public ObservableCollection<AppointmentItem> DisplayedAppointments { get; } = new();

        // ──────────────────────────────────────────────────────
        // FILTER STATE
        // ──────────────────────────────────────────────────────
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ApplyFilters(); }
        }

        // ──────────────────────────────────────────────────────
        // ROW COUNT  (reflects DisplayedAppointments — i.e. filtered)
        // ──────────────────────────────────────────────────────
        public int TotalAppointments => DisplayedAppointments.Count;

        // ──────────────────────────────────────────────────────
        // LOAD FROM DB  (maps PatientProcedure list → AppointmentItem list)
        // ──────────────────────────────────────────────────────
        public void LoadFromDb(IEnumerable<(
            string AppointmentId,
            string PatientId,
            string PatientName,
            string Gender,
            string ProcedureName,
            string Status,
            DateTime? AppointmentDate,
            DateTime? ProcedureDate)> appointments)
        {
            _allAppointments.Clear();

            foreach (var a in appointments)
                _allAppointments.Add(BuildItem(
                    a.AppointmentId,
                    a.PatientId,
                    a.PatientName,
                    a.Gender,
                    a.ProcedureName,
                    a.Status,
                    a.AppointmentDate,
                    a.ProcedureDate));

            ApplyFilters();
        }

        // ──────────────────────────────────────────────────────
        // FACTORY HELPER
        // ──────────────────────────────────────────────────────
        public static AppointmentItem BuildItem(
            string appointmentId,
            string patientId,
            string patientName,
            string gender,
            string procedureName,
            string status,
            DateTime? appointmentDate,
            DateTime? procedureDate)
        {
            var parts = patientName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var initials = parts.Length >= 2
                ? $"{parts[0][0]}{parts[^1][0]}"
                : patientName.Length > 0 ? patientName[0].ToString() : "?";

            var avatarColor = gender switch
            {
                "Female" => new SolidColorBrush(Color.FromArgb(255, 194, 24, 91)),
                "Male" => new SolidColorBrush(Color.FromArgb(255, 0, 120, 212)),
                _ => new SolidColorBrush(Color.FromArgb(255, 91, 45, 142)),
            };

            var dateDisplay = appointmentDate.HasValue
                ? appointmentDate.Value.ToString("MMM dd, yyyy")
                : "Not scheduled";

            var procDateDisplay = procedureDate.HasValue
                ? procedureDate.Value.ToString("MMM dd, yyyy")
                : string.Empty;

            return new AppointmentItem
            {
                AppointmentId = appointmentId,
                PatientId = patientId,
                PatientName = patientName,
                Initials = initials.ToUpper(),
                AvatarColor = avatarColor,
                ProcedureName = procedureName,
                Status = status,
                AppointmentDate = dateDisplay,
                ProcedureDate = procDateDisplay,
            };
        }

        // ──────────────────────────────────────────────────────
        // FILTERS
        // ──────────────────────────────────────────────────────
        public void ApplyFilters()
        {
            var filtered = _allAppointments.Where(a =>
                string.IsNullOrEmpty(SearchText)
                || a.PatientName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                || a.ProcedureName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
            );

            DisplayedAppointments.Clear();
            foreach (var a in filtered)
                DisplayedAppointments.Add(a);

            OnPropertyChanged(nameof(TotalAppointments));
        }

        // ──────────────────────────────────────────────────────
        // LOOKUP HELPER
        // ──────────────────────────────────────────────────────
        public AppointmentItem? FindAppointment(string appointmentId) =>
            _allAppointments.FirstOrDefault(a => a.AppointmentId == appointmentId);

        // ──────────────────────────────────────────────────────
        // INPC
        // ──────────────────────────────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}