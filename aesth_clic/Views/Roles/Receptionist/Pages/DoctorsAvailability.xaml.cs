using aesth_clic.Tenant.Controller;
using aesth_clic.Utils;
using aesth_clic.ViewModels.Receptionist;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;


namespace aesth_clic.Views.Roles.Receptionist.Pages
{
    // ── UI display model ───────────────────────────────────────────────────────
    public class DoctorAvailabilityItem
    {
        public string DoctorId { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;

        // "Available" or "Busy"  (normalized from DTO's lowercase values)
        public string Status { get; set; } = string.Empty;

        // Derived display properties
        public string StatusBackground { get; set; } = string.Empty;
        public string StatusForeground { get; set; } = string.Empty;
        public Windows.UI.Color StatusDotColor { get; set; }
    }

    // ── Page ───────────────────────────────────────────────────────────────────
    public sealed partial class DoctorsAvailability : Page
    {
        private readonly DoctorsAvailabilityViewModel _vm = new();
        private readonly UserController _userController;

        public DoctorsAvailability()
        {
            InitializeComponent();

            _userController = App.Services.GetRequiredService<UserController>();

            DoctorListControl.ItemsSource = _vm.DisplayedDoctors;

            _vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName
                    is nameof(DoctorsAvailabilityViewModel.TotalDoctors)
                    or nameof(DoctorsAvailabilityViewModel.AvailableDoctors)
                    or nameof(DoctorsAvailabilityViewModel.BusyDoctors))
                    UpdateRowCount();
            };

            _ = LoadFromDbAsync();
        }

        // ── Data loading ───────────────────────────────────────────────────────
        private async System.Threading.Tasks.Task LoadFromDbAsync()
        {
            try
            {
                var doctors = await _userController.GetDoctorAvailabilityAsync();

                _vm.LoadFromDb(doctors.Select(d => (
                    DoctorId: d.Id.ToString(),
                    FullName: d.FullName,
                    AvailabilityStatus: d.AvailabilityStatus
                )));

                DoctorListControl.ItemsSource = null;
                DoctorListControl.ItemsSource = _vm.DisplayedDoctors;

                UpdateRowCount();
            }
            catch (Exception ex)
            {
                ToastHelper.Error(ToastBar, "Failed to load doctors", ex.Message);
            }
        }

        // ── Row count ──────────────────────────────────────────────────────────
        private void UpdateRowCount()
        {
            if (TxtRowCount is null) return;

            TxtRowCount.Text =
                $"Showing {_vm.TotalDoctors} doctor{(_vm.TotalDoctors != 1 ? "s" : "")}";
        }

        // ── Event handlers ─────────────────────────────────────────────────────
        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            _vm.SearchText = sender.Text ?? string.Empty;
            UpdateRowCount();
        }

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _vm.SelectedStatus = StatusFilter.SelectedIndex switch
            {
                1 => "Available",
                2 => "Busy",
                _ => "All"
            };
            UpdateRowCount();
        }
    }
}