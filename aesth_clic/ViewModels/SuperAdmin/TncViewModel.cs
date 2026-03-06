using aesth_clic.Master.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace aesth_clic.ViewModels.SuperAdmin
{
    internal class TncViewModel : INotifyPropertyChanged
    {
        // ── Collections ───────────────────────────────────────────────────────
        private readonly List<TncMaster> _allEntries = new();
        public ObservableCollection<TncMaster> Entries { get; } = new();

        // ── Loading state ─────────────────────────────────────────────────────
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        // ── Computed display strings ──────────────────────────────────────────
        public string EntryCountDisplay
        {
            get
            {
                int n = Entries.Count;
                return $"{n} {(n == 1 ? "entry" : "entries")}";
            }
        }

        // ── Load from DB rows ─────────────────────────────────────────────────
        public void LoadFromDb(List<TncMaster> rows)
        {
            _allEntries.Clear();
            _allEntries.AddRange(rows);

            Entries.Clear();
            foreach (var row in _allEntries)
                Entries.Add(row);

            OnPropertyChanged(nameof(EntryCountDisplay));
        }

        // ── Mutation helpers (called by code-behind after DB confirms) ─────────
        public void AddEntry(TncMaster entry)
        {
            _allEntries.Add(entry);
            Entries.Add(entry);
            OnPropertyChanged(nameof(EntryCountDisplay));
        }

        public void RemoveEntry(TncMaster entry)
        {
            _allEntries.Remove(entry);
            Entries.Remove(entry);
            OnPropertyChanged(nameof(EntryCountDisplay));
        }

        public void RefreshCount() =>
            OnPropertyChanged(nameof(EntryCountDisplay));

        // ── INotifyPropertyChanged ─────────────────────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}