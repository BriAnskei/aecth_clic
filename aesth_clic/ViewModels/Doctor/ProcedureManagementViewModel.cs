using aesth_clic.Views.Roles.Doctor.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace aesth_clic.ViewModels.Doctor
{
    internal class ProcedureManagementViewModel : INotifyPropertyChanged
    {
        private readonly List<ProcedureItem> _allProcedures = new();
        public ObservableCollection<ProcedureItem> DisplayedProcedures { get; } = new();

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
        // ROW COUNT  (reflects DisplayedProcedures — i.e. filtered)
        // ──────────────────────────────────────────────────────
        public int TotalProcedures => DisplayedProcedures.Count;

        // ──────────────────────────────────────────────────────
        // LOAD FROM DB  (maps PatientProcedure list → ProcedureItem list)
        // Only rows where ProcedureDate is not null are passed in.
        // ──────────────────────────────────────────────────────
        public void LoadFromDb(IEnumerable<(
            string ProcedureItemId,
            string PatientId,
            string PatientName,
            string Gender,
            string ProcedureName,
            DateTime ProcedureDate)> procedures)
        {
            _allProcedures.Clear();

            foreach (var p in procedures)
                _allProcedures.Add(BuildItem(
                    p.ProcedureItemId,
                    p.PatientId,
                    p.PatientName,
                    p.Gender,
                    p.ProcedureName,
                    p.ProcedureDate));

            ApplyFilters();
        }

        // ──────────────────────────────────────────────────────
        // FACTORY HELPER
        // ──────────────────────────────────────────────────────
        public static ProcedureItem BuildItem(
            string procedureItemId,
            string patientId,
            string patientName,
            string gender,
            string procedureName,
            DateTime procedureDate)
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
                ProcedureItemId = procedureItemId,
                PatientId = patientId,
                PatientName = patientName,
                Initials = initials.ToUpper(),
                AvatarColor = avatarColor,
                ProcedureName = procedureName,
                AppointmentDate = procedureDate.ToString("MMM dd, yyyy"),
                AppointmentTime = procedureDate.ToString("hh:mm tt"),
            };
        }

        // ──────────────────────────────────────────────────────
        // FILTERS
        // ──────────────────────────────────────────────────────
        public void ApplyFilters()
        {
            var filtered = _allProcedures.Where(p =>
                string.IsNullOrEmpty(SearchText)
                || p.PatientName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                || p.ProcedureName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
            );

            DisplayedProcedures.Clear();
            foreach (var p in filtered)
                DisplayedProcedures.Add(p);

            OnPropertyChanged(nameof(TotalProcedures));
        }

        // ──────────────────────────────────────────────────────
        // LOOKUP HELPER  (used by MarkDone_Click)
        // ──────────────────────────────────────────────────────
        public ProcedureItem? FindProcedure(string procedureItemId) =>
            _allProcedures.FirstOrDefault(p => p.ProcedureItemId == procedureItemId);

        // ──────────────────────────────────────────────────────
        // REMOVE HELPER  (called after local mark-done until DB wired up)
        // ──────────────────────────────────────────────────────
        public void Remove(string procedureItemId)
        {
            var item = _allProcedures.FirstOrDefault(p => p.ProcedureItemId == procedureItemId);
            if (item is null) return;
            _allProcedures.Remove(item);
            ApplyFilters();
        }

        // ──────────────────────────────────────────────────────
        // INPC
        // ──────────────────────────────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}