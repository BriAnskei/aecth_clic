using aesth_clic.Tenant.Controller;
using aesth_clic.Tenant.Services;
using aesth_clic.Utils;
using aesth_clic.ViewModels.Receptionist;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace aesth_clic.Views.Roles.Receptionist.Pages
{
    public class ServiceItem
    {
        public string ServiceId { get; set; } = string.Empty;
        public string ProcedureName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string FormattedPrice { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
    }

    public sealed partial class ServiceMenu : Page
    {
        private readonly MenuController _menuController;
        private readonly ServiceMenuViewModel _vm;
        private bool _isPopulating = false;

        public ServiceMenu()
        {
            InitializeComponent();

            _menuController = new MenuController(new MenuService());
            _vm = new ServiceMenuViewModel(_menuController);

            ServiceListControl.ItemsSource = _vm.DisplayedServices;

            _vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(ServiceMenuViewModel.IsLoading))
                    UpdateLoadingState(_vm.IsLoading);

                if (e.PropertyName
                    is nameof(ServiceMenuViewModel.TotalServices)
                    or nameof(ServiceMenuViewModel.LowestPrice)
                    or nameof(ServiceMenuViewModel.HighestPrice))
                    UpdateKpiCards();
            };

            _ = LoadDataAsync();
        }

        // ──────────────────────────────────────────────────────
        // LOADING STATE
        // ──────────────────────────────────────────────────────
        private void UpdateLoadingState(bool isLoading)
        {
            KpiGrid.IsHitTestVisible = !isLoading;
            KpiGrid.Opacity = isLoading ? 0.4 : 1.0;

            FilterToolbar.IsHitTestVisible = !isLoading;
            FilterToolbar.Opacity = isLoading ? 0.4 : 1.0;

            SkeletonTable.Visibility = isLoading
                ? Visibility.Visible
                : Visibility.Collapsed;

            RealTable.Visibility = isLoading
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        // ──────────────────────────────────────────────────────
        // INITIAL LOAD
        // ──────────────────────────────────────────────────────
        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                await _vm.LoadFromBackendAsync();
                PopulateDoctorFilter();
                UpdateKpiCards();
            }
            catch (Exception ex)
            {
                ToastHelper.Error(ToastBar, "Failed to load services", ex.Message);
            }
        }

        // ──────────────────────────────────────────────────────
        // DOCTOR FILTER — populated after data loads
        // ──────────────────────────────────────────────────────
        private void PopulateDoctorFilter()
        {
            _isPopulating = true;
            DoctorFilter.Items.Clear();
            DoctorFilter.Items.Add(new ComboBoxItem { Content = "All Doctors", Tag = "All" });

            foreach (var name in _vm.DoctorNames)
                DoctorFilter.Items.Add(new ComboBoxItem { Content = name, Tag = name });

            DoctorFilter.SelectedIndex = 0;
            _isPopulating = false;
        }

        // ──────────────────────────────────────────────────────
        // KPI CARDS
        // ──────────────────────────────────────────────────────
        private void UpdateKpiCards()
        {
            TxtTotalServices.Text = _vm.TotalServices.ToString();
            TxtLowestPrice.Text = $"₱{_vm.LowestPrice:N0}";
            TxtHighestPrice.Text = $"₱{_vm.HighestPrice:N0}";

            TxtRowCount.Text =
                $"Showing {_vm.DisplayedServices.Count} " +
                $"service{(_vm.DisplayedServices.Count == 1 ? "" : "s")}";
        }

        // ──────────────────────────────────────────────────────
        // EVENT HANDLERS
        // ──────────────────────────────────────────────────────
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_vm is null || _isPopulating) return;
            _vm.SearchText = SearchBox.Text;
            UpdateKpiCards();
        }

        private void DoctorFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_vm is null || _isPopulating) return;
            _vm.SelectedDoctor =
                (DoctorFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All";
            UpdateKpiCards();
        }

        private void PriceFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_vm is null || _isPopulating) return;
            _vm.SelectedPriceRange =
                (PriceFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All";
            UpdateKpiCards();
        }
    }
}