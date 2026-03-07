using aesth_clic.Session;
using aesth_clic.Tenant.Controller;
using aesth_clic.Utils;
using aesth_clic.ViewModels.Doctor;
using aesth_clic.Views.Roles.Doctor.Modals;
using aesth_clic.Views.Roles.Receptionist.Modals.PatientProcedure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;
using Windows.UI;

namespace aesth_clic.Views.Roles.Doctor.Pages
{
    // ── UI display model ───────────────────────────────────────────────────────────
    public class AppointmentItem
    {
        public string AppointmentId { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public SolidColorBrush AvatarColor { get; set; } = new(Color.FromArgb(255, 91, 45, 142));
        public string ProcedureName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string AppointmentDate { get; set; } = string.Empty;
        public string ProcedureDate { get; set; } = string.Empty;
    }

    // ── Page ───────────────────────────────────────────────────────────────────────
    public sealed partial class AppointmentManagement : Microsoft.UI.Xaml.Controls.Page
    {
        private readonly AppointmentManagementViewModel _vm = new();
        private readonly PatientProcedureController _procedureController;

        public AppointmentManagement()
        {
            InitializeComponent();

            _procedureController = App.Services.GetRequiredService<PatientProcedureController>();

            AppointmentListControl.ItemsSource = _vm.DisplayedAppointments;

            _vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is nameof(AppointmentManagementViewModel.TotalAppointments))
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

                var appointments = await _procedureController.GetDoctorAppointmentsAsync(doctorId);

                _vm.LoadFromDb(appointments.Select(a => (
                    AppointmentId: a.Id.ToString(),
                    PatientId: a.PatientId.ToString(),
                    PatientName: a.Patient?.FullName ?? "Unknown Patient",
                    Gender: a.Patient?.Gender ?? string.Empty,
                    ProcedureName: a.ServiceMenu?.Name ?? "Unknown Procedure",
                    Status: a.Status,
                    AppointmentDate: a.AppointmentDate,
                    ProcedureDate: a.ProcedureDate
                )));

                AppointmentListControl.ItemsSource = null;
                AppointmentListControl.ItemsSource = _vm.DisplayedAppointments;

                UpdateRowCount();
            }
            catch (Exception ex)
            {
                ToastHelper.Error(ToastBar, "Failed to load appointments", ex.Message);
            }
        }

        // ── Row Count ──────────────────────────────────────────────────────────────
        private void UpdateRowCount()
        {
            if (TxtRowCount is null) return;
            var count = _vm.TotalAppointments;
            TxtRowCount.Text = $"Showing {count} appointment{(count != 1 ? "s" : "")}";
        }

        // ── Search ─────────────────────────────────────────────────────────────────
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _vm.SearchText = (sender as Microsoft.UI.Xaml.Controls.TextBox)?.Text ?? string.Empty;
        }

        // ── Set Date ───────────────────────────────────────────────────────────────
        private async void SetDate_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
            var record = _vm.FindAppointment(item.Tag?.ToString() ?? string.Empty);
            if (record is null) return;

            var dlg = new SetProcedureDate(_procedureController) { XamlRoot = this.XamlRoot };
            dlg.Load(
                patientProcedureId: int.Parse(record.AppointmentId),
                patientName: record.PatientName,
                initials: record.Initials,
                avatarColor: record.AvatarColor,
                procedureName: record.ProcedureName);

            await dlg.ShowAsync();

            if (!dlg.Saved && dlg.SaveError is null) return;

            if (dlg.SaveError is not null)
            {
                ToastHelper.Error(ToastBar, "Failed to set procedure date", dlg.SaveError.Message);
                return;
            }

            await LoadFromDbAsync();
            ToastHelper.Success(ToastBar, "Procedure date set",
                $"{record.PatientName}'s procedure date has been saved.");
        }

        // ── View ───────────────────────────────────────────────────────────────────
        private async void ViewAppointment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
            var record = _vm.FindAppointment(item.Tag?.ToString() ?? string.Empty);
            if (record is null) return;

            var dlg = new ViewAppointmentDetails { XamlRoot = this.XamlRoot };
            dlg.Load(
                patientName: record.PatientName,
                initials: record.Initials,
                avatarColor: record.AvatarColor,
                procedureName: record.ProcedureName,
                status: record.Status,
                appointmentDate: record.AppointmentDate,
                procedureDate: record.ProcedureDate);

            await dlg.ShowAsync();
        }

        // ── Cancel ─────────────────────────────────────────────────────────────────
        private async void CancelAppointment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
            var record = _vm.FindAppointment(item.Tag?.ToString() ?? string.Empty);
            if (record is null) return;

            var avatarHex = record.AvatarColor.Color is { } c
                ? $"#{c.R:X2}{c.G:X2}{c.B:X2}"
                : "#5B2D8E";

            var (statusLabel, statusColor) = record.Status.ToLower() switch
            {
                "completed" => ("Completed", "#2E7D32"),
                "scheduled" => ("Scheduled", "#0078D4"),
                _ => ("Pending", "#F59E0B"),
            };

            var dlg = new CancelProcedureConfirmation(
                patientName: record.PatientName,
                initials: record.Initials,
                avatarColor: avatarHex,
                procedureName: record.ProcedureName,
                status: statusLabel,
                statusColor: statusColor,
                cost: string.Empty
            )
            { XamlRoot = this.XamlRoot };

            await dlg.ShowAsync();

            if (!dlg.Confirmed) return;

            try
            {
                await _procedureController.DeletePatientProcedureAsync(
                    int.Parse(record.AppointmentId));

                await LoadFromDbAsync();
                ToastHelper.Success(ToastBar, "Appointment cancelled",
                    $"{record.PatientName}'s {record.ProcedureName} has been cancelled.");
            }
            catch (Exception ex)
            {
                ToastHelper.Error(ToastBar, "Failed to cancel appointment", ex.Message);
            }
        }
    }
}