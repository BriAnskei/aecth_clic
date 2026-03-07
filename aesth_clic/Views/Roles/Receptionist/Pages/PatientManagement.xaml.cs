using aesth_clic.Tenant.Controller;
using aesth_clic.Utils;
using aesth_clic.ViewModels.Receptionist;
using aesth_clic.Views.Roles.Receptionist.Modals;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;
using Windows.UI;

namespace aesth_clic.Views.Roles.Receptionist.Pages
{
    // ── UI display model (separate from domain Patient model) ──────────────────────
    public class PatientItem
    {
        public string PatientId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public SolidColorBrush AvatarColor { get; set; } = new(Color.FromArgb(255, 91, 45, 142));
        public SolidColorBrush GenderBadgeColor { get; set; } = new(Color.FromArgb(255, 237, 228, 249));
        public SolidColorBrush GenderBadgeForeground { get; set; } = new(Color.FromArgb(255, 91, 45, 142));
    }

    // ── Page ───────────────────────────────────────────────────────────────────────
    public sealed partial class PatientManagement : Page
    {
        private readonly PatientManagementViewModel _vm = new();
        private readonly PatientController _patientController;

        public PatientManagement()
        {
            InitializeComponent();

            _patientController = App.Services.GetRequiredService<PatientController>();

            PatientListControl.ItemsSource = _vm.DisplayedPatients;

            _vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName
                    is nameof(PatientManagementViewModel.TotalPatients)
                    or nameof(PatientManagementViewModel.MalePatients)
                    or nameof(PatientManagementViewModel.FemalePatients))
                    UpdateKpiCards();
            };

            _ = LoadFromDbAsync();
        }

        // ── Data loading ───────────────────────────────────────────────────────────
        private async System.Threading.Tasks.Task LoadFromDbAsync()
        {
            try
            {
                var patients = await _patientController.GetAllPatientsAsync();

                _vm.LoadFromDb(patients.Select(p => (
                    PatientId: p.Id.ToString(),
                    FullName: p.FullName,
                    Email: p.Email,
                    Phone: p.PhoneNumber,
                    Gender: p.Gender,
                    Age: p.Age,
                    Address: p.Address
                )));

                PatientListControl.ItemsSource = null;
                PatientListControl.ItemsSource = _vm.DisplayedPatients;

                UpdateKpiCards();
            }
            catch (Exception ex)
            {
                ToastHelper.Error(ToastBar, "Failed to load patients", ex.Message);
            }
        }

        // ── KPI Cards ──────────────────────────────────────────────────────────────
        private void UpdateKpiCards()
        {
            if (TxtTotalPatients is null || TxtMalePatients is null ||
                TxtFemalePatients is null || TxtRowCount is null)
                return;

            TxtTotalPatients.Text = _vm.TotalPatients.ToString();
            TxtMalePatients.Text = _vm.MalePatients.ToString();
            TxtFemalePatients.Text = _vm.FemalePatients.ToString();
            TxtRowCount.Text =
                $"Showing {_vm.TotalPatients} patient{(_vm.TotalPatients != 1 ? "s" : "")}";
        }

        // ── Search + Filters ───────────────────────────────────────────────────────
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _vm.SearchText = (sender as TextBox)?.Text ?? string.Empty;
            UpdateKpiCards();
        }

        private void GenderFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _vm.SelectedGender =
                (GenderFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All";
            UpdateKpiCards();
        }

        private void SortOrder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _vm.SelectedSort =
                (SortOrder.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "AZ";
            UpdateKpiCards();
        }

        // ── Add Patient ────────────────────────────────────────────────────────────
        private async void AddPatientButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddEditPatient(_patientController) { XamlRoot = XamlRoot };
            await dialog.ShowAsync();

            if (dialog.Result is null && dialog.SaveError is null) return;

            if (dialog.SaveError is not null)
            {
                ToastHelper.Error(ToastBar, "Failed to add patient", dialog.SaveError.Message);
                return;
            }

            await LoadFromDbAsync();
            ToastHelper.Success(ToastBar, "Patient added",
                $"{dialog.Result!.FullName} has been added successfully.");
        }

        // ── Edit Patient ───────────────────────────────────────────────────────────
        private async void EditPatient_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
            var patientId = item.Tag?.ToString();
            var patient = _vm.FindPatient(patientId ?? string.Empty);
            if (patient is null) return;

            var dialog = new AddEditPatient(_patientController) { XamlRoot = XamlRoot };
            dialog.LoadForEdit(
                int.Parse(patient.PatientId),
                patient.FullName,
                patient.Email,
                patient.Phone,
                patient.Age,
                patient.Gender,
                patient.Address);
            await dialog.ShowAsync();

            if (dialog.Result is null && dialog.SaveError is null) return;

            if (dialog.SaveError is not null)
            {
                ToastHelper.Error(ToastBar, "Failed to update patient", dialog.SaveError.Message);
                return;
            }

            await LoadFromDbAsync();
            ToastHelper.Success(ToastBar, "Patient updated",
                $"{dialog.Result!.FullName} has been updated successfully.");
        }

        // ── Delete Patient ─────────────────────────────────────────────────────────
        private async void DeletePatient_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
            var patientId = item.Tag?.ToString();
            var patient = _vm.FindPatient(patientId ?? string.Empty);
            if (patient is null) return;

            var dlg = new DeletePatient(patient, _patientController) { XamlRoot = XamlRoot };
            await dlg.ShowAsync();

            if (!dlg.Confirmed) return;

            if (dlg.SaveError is not null)
            {
                ToastHelper.Error(ToastBar, "Failed to delete patient", dlg.SaveError.Message);
                return;
            }

            await LoadFromDbAsync();
            ToastHelper.Success(ToastBar, "Patient deleted",
                $"{patient.FullName} has been permanently deleted.");
        }
    }
}