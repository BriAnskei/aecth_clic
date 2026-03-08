using aesth_clic.Tenant.Controller;
using aesth_clic.Tenant.Model;
using aesth_clic.Utils;
using aesth_clic.ViewModels.Receptionist;
using aesth_clic.Views.Roles.Receptionist.Modals;
using aesth_clic.Views.Roles.Receptionist.Modals.PatientProcedure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using System.Globalization;
using System.Linq;
using Windows.UI;

namespace aesth_clic.Views.Roles.Receptionist.Pages
{
    // ── Converter ──────────────────────────────────────────────────────────────
    public class StringToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string hex)
            {
                hex = hex.TrimStart('#');
                if (hex.Length == 6) hex = "FF" + hex;
                if (uint.TryParse(hex, NumberStyles.HexNumber, null, out uint argb))
                {
                    return new SolidColorBrush(Color.FromArgb(
                        (byte)(argb >> 24),
                        (byte)(argb >> 16),
                        (byte)(argb >> 8),
                        (byte)(argb)));
                }
            }
            return new SolidColorBrush(Color.FromArgb(255, 91, 45, 142));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    // ── Data Model ─────────────────────────────────────────────────────────────
    public class AppointmentItem
    {
        public string AppointmentId { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string AvatarColor { get; set; } = "#5B2D8E";
        public string DoctorName { get; set; } = string.Empty;
        public string ProcedureName { get; set; } = string.Empty;
        public string AppointmentDate { get; set; } = string.Empty;
        public string AppointmentTime { get; set; } = string.Empty;

        // Raw values needed for edit mode pre-fill
        public int RawProcedureRecordId { get; set; }
        public int RawDoctorId { get; set; }
        public DateTime RawAppointmentDate { get; set; }
    }

    // ── Page ───────────────────────────────────────────────────────────────────
    public sealed partial class AppointmentManagement : Page
    {
        private readonly AppointmentManagementViewModel _vm = new();
        private readonly PatientProcedureController _procedureController;
        private readonly UserController _userController;

        public AppointmentManagement()
        {
            InitializeComponent();

            _procedureController = App.Services.GetRequiredService<PatientProcedureController>();
            _userController = App.Services.GetRequiredService<UserController>();

            AppointmentListControl.ItemsSource = _vm.DisplayedAppointments;

            _vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is nameof(AppointmentManagementViewModel.DisplayedCount))
                    UpdateRowCount();
            };

            _ = LoadFromDbAsync();
        }

        // ── Data loading ───────────────────────────────────────────────────────
        private async System.Threading.Tasks.Task LoadFromDbAsync()
        {
            try
            {
                var records = await _procedureController.GetCurrentAppointmentsAsync();
                _vm.LoadFromDb(records.Select(MapToItem));

                AppointmentListControl.ItemsSource = null;
                AppointmentListControl.ItemsSource = _vm.DisplayedAppointments;

                UpdateRowCount();
            }
            catch (Exception ex)
            {
                ToastHelper.Error(ToastBar, "Failed to load appointments", ex.Message);
            }
        }

        // ── Domain → UI model mapping ──────────────────────────────────────────
        private static AppointmentItem MapToItem(PatientProcedure p)
        {
            var fullName = p.Patient?.FullName ?? "Unknown";
            var gender = p.Patient?.Gender ?? string.Empty;

            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var initials = parts.Length >= 2
                ? $"{parts[0][0]}{parts[^1][0]}"
                : fullName.Length > 0 ? fullName[0].ToString() : "?";

            var avatarColor = gender switch
            {
                "Female" => "#C2185B",
                "Male" => "#0078D4",
                _ => "#5B2D8E"
            };

            var doctorName = p.User?.FullName is { Length: > 0 } name ? name : "—";

            var appointmentDate = p.AppointmentDate.HasValue
                ? p.AppointmentDate.Value.ToString("MMM dd, yyyy")
                : "—";

            var appointmentTime = p.AppointmentDate.HasValue
                ? p.AppointmentDate.Value.ToString("hh:mm tt")
                : string.Empty;

            return new AppointmentItem
            {
                AppointmentId = p.Id.ToString(),
                PatientId = p.PatientId.ToString(),
                PatientName = fullName,
                Initials = initials.ToUpper(),
                AvatarColor = avatarColor,
                DoctorName = doctorName,
                ProcedureName = p.ServiceMenu?.Name ?? "Unknown",
                AppointmentDate = appointmentDate,
                AppointmentTime = appointmentTime,
                RawProcedureRecordId = p.Id,
                RawDoctorId = p.AssignedDoctorId ?? 0,
                RawAppointmentDate = p.AppointmentDate ?? DateTime.Today,
            };
        }

        // ── Row count ──────────────────────────────────────────────────────────
        private void UpdateRowCount()
        {
            if (TxtRowCount is null) return;
            var count = _vm.DisplayedCount;
            TxtRowCount.Text = $"Showing {count} appointment{(count == 1 ? "" : "s")}";
        }

        // ── Search ─────────────────────────────────────────────────────────────
        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            _vm.SearchText = sender.Text;
        }

        // ── Kebab (no-op — flyout is declared in XAML) ─────────────────────────
        private void KebabMenu_Click(object sender, RoutedEventArgs e) { }

        // ── Cancel Appointment ─────────────────────────────────────────────────
        private async void CancelAppointment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;

            var appointmentId = item.Tag?.ToString() ?? string.Empty;
            var appt = _vm.FindAppointment(appointmentId);
            if (appt is null) return;

            var dialog = new CancelProcedureConfirmation(
                appt.PatientName,
                appt.Initials,
                appt.AvatarColor,
                appt.ProcedureName,
                "Scheduled",
                "#0078D4",
                string.Empty)
            { XamlRoot = XamlRoot };

            await dialog.ShowAsync();
            if (!dialog.Confirmed) return;

            try
            {
                await _procedureController.DeletePatientProcedureAsync(
                    int.Parse(appt.AppointmentId));

                await LoadFromDbAsync();
                ToastHelper.Warning(ToastBar, "Appointment cancelled",
                    $"{appt.ProcedureName} for {appt.PatientName} has been cancelled.");
            }
            catch (Exception ex)
            {
                ToastHelper.Error(ToastBar, "Failed to cancel appointment", ex.Message);
            }
        }

        // ── Add Appointment ────────────────────────────────────────────────────
        private async void AddAppointment_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddAppointmentModal(
                _procedureController,
                _userController)
            { XamlRoot = XamlRoot };

            await dialog.ShowAsync();

            if (dialog.Result is null && dialog.SaveError is null) return;

            if (dialog.SaveError is not null)
            {
                ToastHelper.Error(ToastBar, "Failed to add appointment", dialog.SaveError.Message);
                return;
            }

            await LoadFromDbAsync();
            ToastHelper.Success(ToastBar, "Appointment scheduled",
                $"{dialog.Result!.ProcedureName} for {dialog.Result.PatientName} assigned to Dr. {dialog.Result.DoctorName} on {dialog.Result.AppointmentDate:MMM dd, yyyy}.");
        }
    }
}