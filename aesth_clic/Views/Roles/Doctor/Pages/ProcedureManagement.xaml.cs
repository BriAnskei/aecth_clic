using aesth_clic.Session;
using aesth_clic.Tenant.Controller;
using aesth_clic.Utils;
using aesth_clic.ViewModels.Doctor;
using aesth_clic.Views.Roles.Doctor.Modals;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;



namespace aesth_clic.Views.Roles.Doctor.Pages
{
    public class ProcedureItem
    {
        public string ProcedureItemId { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string AvatarColor { get; set; } = "#5B2D8E";
        public string ProcedureName { get; set; } = string.Empty;
        public string AppointmentDate { get; set; } = string.Empty;
        public string AppointmentTime { get; set; } = string.Empty;
    }

    public sealed partial class ProcedureManagement : Page
    {
        private readonly ProcedureManagementViewModel _vm = new();
        private readonly PatientProcedureController _procedureController;

        public ProcedureManagement()
        {
            InitializeComponent();

            _procedureController = App.Services.GetRequiredService<PatientProcedureController>();

            ProcedureListControl.ItemsSource = _vm.DisplayedProcedures;

            _vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is nameof(ProcedureManagementViewModel.TotalProcedures))
                    UpdateRowCount();
            };

            _ = LoadFromDbAsync();
        }

        // ── Data loading ───────────────────────────────────────────────────────────
        private async System.Threading.Tasks.Task LoadFromDbAsync()
        {
            try
            {
                var doctorId = AppSession.Instance.CurrentUser?.Id
                    ?? throw new InvalidOperationException("No logged-in user found.");

                var procedures = await _procedureController.getProceduresByDoctorsId(doctorId);

                // Only show rows where ProcedureDate has been set by the doctor
                _vm.LoadFromDb(procedures
                    .Where(p => p.ProcedureDate.HasValue)
                    .Select(p => (
                        ProcedureItemId: p.Id.ToString(),
                        PatientId: p.PatientId.ToString(),
                        PatientName: p.Patient?.FullName ?? "Unknown Patient",
                        Gender: p.Patient?.Gender ?? string.Empty,
                        ProcedureName: p.ServiceMenu?.Name ?? "Unknown Procedure",
                        ProcedureDate: p.ProcedureDate!.Value
                    )));

                ProcedureListControl.ItemsSource = null;
                ProcedureListControl.ItemsSource = _vm.DisplayedProcedures;

                UpdateRowCount();
            }
            catch (Exception ex)
            {
                ToastHelper.Error(ToastBar, "Failed to load procedures", ex.Message);
            }
        }

        // ── Row Count ──────────────────────────────────────────────────────────────
        private void UpdateRowCount()
        {
            if (TxtRowCount is null) return;
            var count = _vm.TotalProcedures;
            TxtRowCount.Text = $"Showing {count} procedure{(count != 1 ? "s" : "")}";
        }

        // ── Search ─────────────────────────────────────────────────────────────────
        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            _vm.SearchText = sender.Text ?? string.Empty;
        }

        // ── Mark Done ──────────────────────────────────────────────────────────────
        private async void MarkDone_Click(object sender, RoutedEventArgs e)
        {
            var id = (sender as Button)?.Tag?.ToString();
            var record = _vm.FindProcedure(id ?? string.Empty);
            if (record is null) return;

            var modal = new MarkDoneModal(
                procedureItemId: record.ProcedureItemId,
                patientName: record.PatientName,
                patientGender: string.Empty,          // populate from your model if available
                procedureName: record.ProcedureName,
                appointmentDate: record.AppointmentDate)
            {
                XamlRoot = XamlRoot
            };

            await modal.ShowAsync();

            if (modal.SaveError is not null)
            {
                ToastHelper.Error(ToastBar, "Failed to complete procedure", modal.SaveError.Message);
                return;
            }

            if (modal.Result is not null)
            {
                _vm.Remove(record.ProcedureItemId);
                UpdateRowCount();

                var medicineNames = string.Join(", ", modal.Result.Medicines.Select(m => m.Name));
                ToastHelper.Success(ToastBar,
                    $"{record.ProcedureName} marked as complete",
                    $"Reseta: {medicineNames}");
            }
        }
    }
}