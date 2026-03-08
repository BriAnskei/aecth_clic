using aesth_clic.Views.Roles.Pharmacist.Pages;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace aesth_clic.ViewModels.Pharmacist
{
    internal class InventoryManagementViewModel : INotifyPropertyChanged
    {
        private readonly List<InventoryItem> _allItems = new();
        public ObservableCollection<InventoryItem> DisplayedItems { get; } = new();

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
        // KPI COUNTERS  (always from full unfiltered _allItems)
        // ──────────────────────────────────────────────────────
        public int TotalMedicines => _allItems.Count;
        public int SufficientCount => _allItems.Count(m => m.StatusLabel == "Sufficient");
        public int LowStockCount => _allItems.Count(m => m.StatusLabel == "Low Stock");

        // ──────────────────────────────────────────────────────
        // LOAD FROM DB
        // ──────────────────────────────────────────────────────
        public void LoadFromDb(IEnumerable<(
            string MedicineId,
            string MedicineName,
            int StockQuantity,
            string LastStockIn,
            string Unit,
            string ExpiryDate)> medicines)
        {
            _allItems.Clear();

            foreach (var m in medicines)
                _allItems.Add(new InventoryItem
                {
                    MedicineId = m.MedicineId,
                    MedicineName = m.MedicineName,
                    StockQuantity = m.StockQuantity,
                    LastStockIn = m.LastStockIn,
                    Unit = m.Unit,
                    ExpiryDate = m.ExpiryDate,
                });

            ApplyFilters();
            NotifyKpis();
        }

        // ──────────────────────────────────────────────────────
        // FILTERS
        // ──────────────────────────────────────────────────────
        public void ApplyFilters()
        {
            var filtered = _allItems.Where(m =>
                (string.IsNullOrEmpty(SearchText)
                    || m.MedicineName.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase))
                && (SelectedStatus == "All" || m.StatusLabel == SelectedStatus)
            ).OrderBy(m => m.MedicineName);

            DisplayedItems.Clear();
            foreach (var m in filtered)
                DisplayedItems.Add(m);

            OnPropertyChanged(nameof(DisplayedItems));
        }

        // ──────────────────────────────────────────────────────
        // CRUD HELPERS
        // ──────────────────────────────────────────────────────

        /// <summary>Returns the item from the master list, or null.</summary>
        public InventoryItem? FindItem(string medicineId) =>
            _allItems.FirstOrDefault(m => m.MedicineId == medicineId);

        // ──────────────────────────────────────────────────────
        // KPI NOTIFY
        // ──────────────────────────────────────────────────────
        public void NotifyKpis()
        {
            OnPropertyChanged(nameof(TotalMedicines));
            OnPropertyChanged(nameof(SufficientCount));
            OnPropertyChanged(nameof(LowStockCount));
        }

        // ──────────────────────────────────────────────────────
        // INPC
        // ──────────────────────────────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}