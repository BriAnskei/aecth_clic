using aesth_clic.Tenant.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace aesth_clic.ViewModels.Admin
{
    internal class TncTenantViewModel : INotifyPropertyChanged
    {
        // ── Collections ───────────────────────────────────────────────────────

        // Master (read-only, from SuperAdmin)
        private readonly List<TncTenant> _allMasterEntries = new();
        public ObservableCollection<TncTenant> MasterEntries { get; } = new();

        // Tenant (admin-managed, clinic-specific)
        private readonly List<TncTenant> _allTenantEntries = new();
        public ObservableCollection<TncTenant> TenantEntries { get; } = new();

        // ── Loading state ─────────────────────────────────────────────────────

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        // ── Computed display strings ──────────────────────────────────────────

        public string MasterEntryCountDisplay
        {
            get
            {
                int n = MasterEntries.Count;
                return $"{n} {(n == 1 ? "provision" : "provisions")}";
            }
        }

        public string TenantEntryCountDisplay
        {
            get
            {
                int n = TenantEntries.Count;
                return $"{n} {(n == 1 ? "entry" : "entries")}";
            }
        }

        // ── Load from DB rows ─────────────────────────────────────────────────

        public void LoadMasterFromDb(List<TncTenant> rows)
        {
            _allMasterEntries.Clear();
            _allMasterEntries.AddRange(rows);

            MasterEntries.Clear();
            foreach (var row in _allMasterEntries)
                MasterEntries.Add(row);

            OnPropertyChanged(nameof(MasterEntryCountDisplay));
        }

        public void LoadTenantFromDb(List<TncTenant> rows)
        {
            _allTenantEntries.Clear();
            _allTenantEntries.AddRange(rows);

            TenantEntries.Clear();
            foreach (var row in _allTenantEntries)
                TenantEntries.Add(row);

            OnPropertyChanged(nameof(TenantEntryCountDisplay));
        }

        // ── Mutation helpers (called by code-behind after DB confirms) ─────────

        public void AddTenantEntry(TncTenant entry)
        {
            _allTenantEntries.Add(entry);
            TenantEntries.Add(entry);
            OnPropertyChanged(nameof(TenantEntryCountDisplay));
        }

        public void RemoveTenantEntry(TncTenant entry)
        {
            _allTenantEntries.Remove(entry);
            TenantEntries.Remove(entry);
            OnPropertyChanged(nameof(TenantEntryCountDisplay));
        }

        public void RefreshCount() =>
            OnPropertyChanged(nameof(TenantEntryCountDisplay));

        // ── INotifyPropertyChanged ─────────────────────────────────────────────

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}