using aesth_clic.Tenant.Controller;
using aesth_clic.Views.Roles.Receptionist.Pages;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace aesth_clic.ViewModels.Receptionist
{
    public class ServiceMenuViewModel : INotifyPropertyChanged
    {
        private readonly MenuController _menuController;

        // ── Full backing list ─────────────────────────────────────
        private readonly List<ServiceItem> _allServices = new();

        // ── Bound to the ItemsControl ─────────────────────────────
        public ObservableCollection<ServiceItem> DisplayedServices { get; } = new();

        // ── KPI values ────────────────────────────────────────────
        private int _totalServices;
        public int TotalServices
        {
            get => _totalServices;
            private set { _totalServices = value; OnPropertyChanged(); }
        }

        private decimal _lowestPrice;
        public decimal LowestPrice
        {
            get => _lowestPrice;
            private set { _lowestPrice = value; OnPropertyChanged(); }
        }

        private decimal _highestPrice;
        public decimal HighestPrice
        {
            get => _highestPrice;
            private set { _highestPrice = value; OnPropertyChanged(); }
        }

        // ── Loading flag ──────────────────────────────────────────
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        // ── Filter state ──────────────────────────────────────────
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; ApplyFilters(); }
        }

        private string _selectedDoctor = "All";
        public string SelectedDoctor
        {
            get => _selectedDoctor;
            set { _selectedDoctor = value; ApplyFilters(); }
        }

        private string _selectedPriceRange = "All";
        public string SelectedPriceRange
        {
            get => _selectedPriceRange;
            set { _selectedPriceRange = value; ApplyFilters(); }
        }

        // ── Distinct doctor list for the filter ComboBox ──────────
        public List<string> DoctorNames { get; private set; } = new();

        // ─────────────────────────────────────────────────────────
        public ServiceMenuViewModel(MenuController menuController)
        {
            _menuController = menuController;
        }

        // ── Load from backend ─────────────────────────────────────
        public async Task LoadFromBackendAsync()
        {
            IsLoading = true;
            try
            {
                var services = await _menuController.GetAllServicesAsync();

                _allServices.Clear();
                foreach (var s in services)
                {
                    string doctorName = s.User is not null
                        ? s.User.FullName ?? "Unknown"
                        : "Unknown";

                    _allServices.Add(new ServiceItem
                    {
                        ServiceId = s.Id.ToString(),
                        ProcedureName = s.Name,
                        Price = (decimal)s.Price,
                        FormattedPrice = $"₱{s.Price:N0}",
                        DoctorName = doctorName,
                    });
                }

                // Build distinct doctor list for the filter dropdown
                DoctorNames = _allServices
                    .Select(x => x.DoctorName)
                    .Distinct()
                    .OrderBy(d => d)
                    .ToList();

                // Compute KPIs from full dataset
                TotalServices = _allServices.Count;
                LowestPrice = _allServices.Any() ? _allServices.Min(s => s.Price) : 0;
                HighestPrice = _allServices.Any() ? _allServices.Max(s => s.Price) : 0;

                ApplyFilters();
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ── Filter logic ──────────────────────────────────────────
        private void ApplyFilters()
        {
            var search = _searchText.Trim().ToLower();

            var filtered = _allServices.Where(s =>
            {
                bool matchSearch = string.IsNullOrEmpty(search)
                    || s.ProcedureName.ToLower().Contains(search);

                bool matchDoctor = _selectedDoctor == "All"
                    || s.DoctorName == _selectedDoctor;

                bool matchPrice = _selectedPriceRange switch
                {
                    "U500" => s.Price < 500m,
                    "500to2000" => s.Price >= 500m && s.Price <= 2000m,
                    "2000to5000" => s.Price > 2000m && s.Price <= 5000m,
                    "A5000" => s.Price > 5000m,
                    _ => true
                };

                return matchSearch && matchDoctor && matchPrice;
            }).ToList();

            DisplayedServices.Clear();
            foreach (var item in filtered)
                DisplayedServices.Add(item);
        }

        // ── INotifyPropertyChanged ────────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}