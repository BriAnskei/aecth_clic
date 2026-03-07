using aesth_clic.Views.Roles.Receptionist.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace aesth_clic.ViewModels.Receptionist
{
    internal class PatientProceduresViewModel : INotifyPropertyChanged
    {
        private readonly List<PatientProcedureItem> _allProcedures = new();
        public ObservableCollection<PatientProcedureItem> DisplayedProcedures { get; } = new();

        // ── KPI counters — always reflect the FULL unfiltered list ─────────────
        public int TotalProcedures => _allProcedures.Count;
        public int PendingProcedures => _allProcedures.Count(p => p.Status == "Pending");
        public int ScheduledProcedures => _allProcedures.Count(p => p.Status == "Scheduled");
        public int CompletedProcedures => _allProcedures.Count(p => p.Status == "Completed");

        // ── Displayed row count (filtered) ─────────────────────────────────────
        public int DisplayedCount => DisplayedProcedures.Count;

        // ── Filter state ───────────────────────────────────────────────────────
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

        // ── Load ───────────────────────────────────────────────────────────────
        public void LoadFromDb(IEnumerable<PatientProcedureItem> items)
        {
            _allProcedures.Clear();
            foreach (var item in items)
                _allProcedures.Add(item);

            ApplyFilters();
            NotifyKpis();
        }

        // ── Filters ────────────────────────────────────────────────────────────
        public void ApplyFilters()
        {
            var search = SearchText.Trim().ToLower();

            var filtered = _allProcedures.Where(p =>
                (string.IsNullOrEmpty(search)
                    || p.PatientName.ToLower().Contains(search)
                    || p.ProcedureName.ToLower().Contains(search))
                && (SelectedStatus == "All" || p.Status == SelectedStatus)
            );

            DisplayedProcedures.Clear();
            foreach (var p in filtered)
                DisplayedProcedures.Add(p);

            OnPropertyChanged(nameof(DisplayedCount));
        }

        // ── Helpers ────────────────────────────────────────────────────────────
        public PatientProcedureItem? FindProcedure(string recordId) =>
            _allProcedures.FirstOrDefault(p => p.ProcedureRecordId == recordId);

        private void NotifyKpis()
        {
            OnPropertyChanged(nameof(TotalProcedures));
            OnPropertyChanged(nameof(PendingProcedures));
            OnPropertyChanged(nameof(ScheduledProcedures));
            OnPropertyChanged(nameof(CompletedProcedures));
        }

        // ── INPC ───────────────────────────────────────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}