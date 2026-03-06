using aesth_clic.Tenant.Controller;
using aesth_clic.Views.Roles.Doctor.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// Alias the backend model to avoid collision with the ServiceMenu Page class
using ServiceMenuModel = aesth_clic.Tenant.Model.ServiceMenu;

namespace aesth_clic.ViewModels.Doctor
{
    internal class ServiceMenuViewModel : INotifyPropertyChanged
    {
        private readonly MenuController _menuController;
        private readonly List<ServiceMenuItem> _allServices = new();

        public ObservableCollection<ServiceMenuItem> DisplayedServices { get; } = new();

        // ──────────────────────────────────────────────────────
        // CONSTRUCTOR
        // ──────────────────────────────────────────────────────
        internal ServiceMenuViewModel(MenuController menuController)
        {
            _menuController = menuController;
        }

        // ──────────────────────────────────────────────────────
        // LOADING STATE
        // ──────────────────────────────────────────────────────
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        // ──────────────────────────────────────────────────────
        // FILTERS
        // ──────────────────────────────────────────────────────
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ApplyFilters(); }
        }

        // ──────────────────────────────────────────────────────
        // COMPUTED KPI PROPS  (reflect _allServices, not filtered view)
        // ──────────────────────────────────────────────────────
        public int TotalServices => _allServices.Count;

        public decimal AvgPrice =>
            _allServices.Count > 0 ? _allServices.Average(s => s.RawPrice) : 0m;

        // ──────────────────────────────────────────────────────
        // LOAD FROM BACKEND
        // ──────────────────────────────────────────────────────
        public async Task LoadFromBackendAsync()
        {
            IsLoading = true;
            try
            {
                List<ServiceMenuModel> services = await _menuController.GetAllServicesAsync();

                _allServices.Clear();
                foreach (var s in services)
                    _allServices.Add(BuildItem(s));

                ApplyFilters();
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ──────────────────────────────────────────────────────
        // FILTERS
        // ──────────────────────────────────────────────────────
        public void ApplyFilters()
        {
            var search = SearchText.Trim();

            var filtered = _allServices.Where(s =>
                string.IsNullOrEmpty(search)
                || s.ProcedureName.Contains(search, StringComparison.OrdinalIgnoreCase)
                || s.AddedByDoctor.Contains(search, StringComparison.OrdinalIgnoreCase)
            ).ToList();

            DisplayedServices.Clear();
            foreach (var item in filtered)
                DisplayedServices.Add(item);

            OnPropertyChanged(nameof(TotalServices));
            OnPropertyChanged(nameof(AvgPrice));
        }

        // ──────────────────────────────────────────────────────
        // CRUD HELPERS
        // ──────────────────────────────────────────────────────
        public ServiceMenuItem? FindService(int serviceId) =>
            _allServices.FirstOrDefault(s => s.ServiceId == serviceId);

        public void AddService(ServiceMenuModel created)
        {
            _allServices.Add(BuildItem(created));
            ApplyFilters();
        }

        public void UpdateService(int serviceId, string newName, decimal newPrice)
        {
            var item = _allServices.FirstOrDefault(s => s.ServiceId == serviceId);
            if (item is null) return;

            item.ProcedureName = newName;
            item.RawPrice = newPrice;
            item.Price = $"₱{newPrice:N0}";
            ApplyFilters();
        }

        public void DeleteService(int serviceId)
        {
            var item = _allServices.FirstOrDefault(s => s.ServiceId == serviceId);
            if (item is null) return;

            _allServices.Remove(item);
            ApplyFilters();
        }

        // ──────────────────────────────────────────────────────
        // BUILDER  (ServiceMenuModel → ServiceMenuItem)
        // ──────────────────────────────────────────────────────
        private static ServiceMenuItem BuildItem(ServiceMenuModel s)
        {
            // Doctor name comes from the User navigation property
            string doctor = s.User?.FullName ?? "Unknown";

            var parts = doctor.Replace("Dr. ", "")
                              .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            string initials = parts.Length >= 2
                ? $"{parts[0][0]}{parts[^1][0]}"
                : parts.Length > 0 ? parts[0][0].ToString() : "?";

            var color = doctor switch
            {
                var d when d.Contains("Maria") => Windows.UI.Color.FromArgb(255, 194, 24, 91),
                var d when d.Contains("Jose") => Windows.UI.Color.FromArgb(255, 0, 120, 212),
                var d when d.Contains("Ana") => Windows.UI.Color.FromArgb(255, 91, 45, 142),
                _ => Windows.UI.Color.FromArgb(255, 91, 45, 142),
            };

            decimal rawPrice = (decimal)s.Price;

            return new ServiceMenuItem
            {
                ServiceId = s.Id,
                ProcedureName = s.Name,
                Price = $"₱{rawPrice:N0}",
                RawPrice = rawPrice,
                AddedByDoctor = doctor,
                DoctorInitials = initials.ToUpper(),
                DoctorAvatarColor = new Microsoft.UI.Xaml.Media.SolidColorBrush(color),
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}