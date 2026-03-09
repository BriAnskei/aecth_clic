using aesth_clic.Tenant.Controller;
using aesth_clic.Utils;
using aesth_clic.ViewModels.Pharmacist;
using aesth_clic.Views.Roles.Pharmacist.Modals;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace aesth_clic.Views.Roles.Pharmacist.Pages
{
    // ── UI display model ──────────────────────────────────────────────────────────
    public class PatientMedicineItem
    {
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string PatientGender { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string AvatarColor { get; set; } = "#5B2D8E";
        public string AssignedDoctor { get; set; } = string.Empty;
        public int TotalMedicine { get; set; }

        public string TotalMedicineDisplay =>
            $"{TotalMedicine} medicine{(TotalMedicine == 1 ? "" : "s")}";
    }

    // ── Page ──────────────────────────────────────────────────────────────────────
    public sealed partial class PatientMedicine : Page
    {
        private readonly PatientMedicineViewModel _vm = new();
        private readonly PrescriptionController _prescriptionController;

        public PatientMedicine()
        {
            InitializeComponent();

            _prescriptionController = App.Services.GetRequiredService<PrescriptionController>();

            PatientMedicineListControl.ItemsSource = _vm.DisplayedItems;

            _ = LoadFromDbAsync();
        }

        // ── Data loading ──────────────────────────────────────────────────────────
        private async System.Threading.Tasks.Task LoadFromDbAsync()
        {
            try
            {
                var prescriptions = await _prescriptionController.GetAllPrescriptionsAsync();

                // Pass full Prescription objects — ViewModel maps them to display items
                // and keeps them internally for FindPrescription() lookups
                _vm.LoadFromDb(prescriptions);

                PatientMedicineListControl.ItemsSource = null;
                PatientMedicineListControl.ItemsSource = _vm.DisplayedItems;

                UpdateRowCount();
            }
            catch (Exception ex)
            {
                ToastHelper.Error(ToastBar, "Failed to load patient medicines", ex.Message);
            }
        }

        // ── Row count label ───────────────────────────────────────────────────────
        private void UpdateRowCount()
        {
            if (TxtRowCount is null) return;
            var count = _vm.DisplayedItems.Count;
            TxtRowCount.Text = $"Showing {count} patient{(count != 1 ? "s" : "")}";
        }

        // ── Search ────────────────────────────────────────────────────────────────
        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            _vm.SearchText = sender.Text ?? string.Empty;
            UpdateRowCount();
        }

        // ── View Details ──────────────────────────────────────────────────────────
        private async void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;

            var patientId = item.Tag?.ToString() ?? string.Empty;
            var record = _vm.FindItem(patientId);
            var prescription = _vm.FindPrescription(patientId);

            if (record is null || prescription is null) return;

            var modal = new PatientPrescriptionModal(
                prescription: prescription,
                controller: _prescriptionController,
                patientName: record.PatientName,
                patientGender: record.PatientGender,
                assignedDoctor: record.AssignedDoctor)
            {
                XamlRoot = XamlRoot
            };

            await modal.ShowAsync();

            // Reload so completed prescriptions disappear from the list
            await LoadFromDbAsync();
        }
    }
}