using aesth_clic.Tenant.Controller;
using aesth_clic.Tenant.Model;
using aesth_clic.Utils;
using aesth_clic.ViewModels.Doctor;
using aesth_clic.Views.Roles.Doctor.Modals;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;
using Windows.UI;

namespace aesth_clic.Views.Roles.Doctor.Pages
{
    // ── UI display model ───────────────────────────────────────────────────────
    public class PatientProcedureItem
    {
        public string ProcedureRecordId { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public SolidColorBrush AvatarColor { get; set; } = new(Color.FromArgb(255, 91, 45, 142));
        public string ProcedureName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public SolidColorBrush StatusBadgeColor { get; set; } = new(Color.FromArgb(255, 237, 228, 249));
        public SolidColorBrush StatusBadgeForeground { get; set; } = new(Color.FromArgb(255, 91, 45, 142));

        public string AppointmentSchedule { get; set; } = string.Empty;
        public string ProcedureSchedule { get; set; } = string.Empty;
        public string Cost { get; set; } = string.Empty;

        // ── Visibility helpers ─────────────────────────────────────────────────
        public Visibility HasAppointmentDate
            => string.IsNullOrEmpty(AppointmentSchedule) ? Visibility.Collapsed : Visibility.Visible;
        public Visibility NoAppointmentDate
            => string.IsNullOrEmpty(AppointmentSchedule) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility HasProcedureDate
            => string.IsNullOrEmpty(ProcedureSchedule) ? Visibility.Collapsed : Visibility.Visible;
        public Visibility NoProcedureDate
            => string.IsNullOrEmpty(ProcedureSchedule) ? Visibility.Visible : Visibility.Collapsed;

        // ── Schedule button enabled only when no appointment date is set yet ───
        public bool IsSchedulable => string.IsNullOrEmpty(AppointmentSchedule);
    }

    // ── Page ───────────────────────────────────────────────────────────────────
    public sealed partial class PatientProcedures : Page
    {
        private readonly PatientProceduresViewModel _vm = new();
        private readonly PatientProcedureController _procedureController;

        public PatientProcedures()
        {
            InitializeComponent();

            _procedureController = App.Services.GetRequiredService<PatientProcedureController>();

            ProcedureListControl.ItemsSource = _vm.DisplayedProcedures;

            _vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName
                    is nameof(PatientProceduresViewModel.TotalProcedures)
                    or nameof(PatientProceduresViewModel.PendingProcedures)
                    or nameof(PatientProceduresViewModel.ScheduledProcedures)
                    or nameof(PatientProceduresViewModel.CompletedProcedures)
                    or nameof(PatientProceduresViewModel.DisplayedCount))
                    UpdateKpiCards();
            };

            _ = LoadFromDbAsync();
        }

        // ── Data loading ───────────────────────────────────────────────────────
        private async System.Threading.Tasks.Task LoadFromDbAsync()
        {
            try
            {
                var records = await _procedureController.GetAllPatientProceduresAsync();
                _vm.LoadFromDb(records.Select(MapToItem));

                ProcedureListControl.ItemsSource = null;
                ProcedureListControl.ItemsSource = _vm.DisplayedProcedures;

                UpdateKpiCards();
            }
            catch (Exception ex)
            {
                ToastHelper.Error(ToastBar, "Failed to load procedures", ex.Message);
            }
        }

        // ── Domain → UI model mapping ──────────────────────────────────────────
        private static PatientProcedureItem MapToItem(PatientProcedure p)
        {
            var fullName = p.Patient?.FullName ?? "Unknown";
            var gender = p.Patient?.Gender ?? string.Empty;

            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var initials = parts.Length >= 2
                ? $"{parts[0][0]}{parts[^1][0]}"
                : fullName.Length > 0 ? fullName[0].ToString() : "?";

            var avatarColor = gender switch
            {
                "Female" => Color.FromArgb(255, 194, 24, 91),
                "Male" => Color.FromArgb(255, 0, 120, 212),
                _ => Color.FromArgb(255, 91, 45, 142),
            };

            var displayStatus = p.Status?.ToLower() switch
            {
                "pending" => "Pending",
                "scheduled" => "Scheduled",
                "completed" => "Completed",
                "cancelled" => "Cancelled",
                _ => p.Status ?? string.Empty
            };

            var (stBg, stFg) = displayStatus switch
            {
                "Completed" => (Color.FromArgb(255, 232, 245, 233), Color.FromArgb(255, 46, 125, 50)),
                "Scheduled" => (Color.FromArgb(255, 227, 242, 253), Color.FromArgb(255, 0, 120, 212)),
                "Cancelled" => (Color.FromArgb(255, 253, 236, 234), Color.FromArgb(255, 197, 15, 31)),
                _ => (Color.FromArgb(255, 255, 248, 225), Color.FromArgb(255, 245, 158, 11)),
            };

            return new PatientProcedureItem
            {
                ProcedureRecordId = p.Id.ToString(),
                PatientId = p.PatientId.ToString(),
                PatientName = fullName,
                Initials = initials.ToUpper(),
                AvatarColor = new SolidColorBrush(avatarColor),
                ProcedureName = p.ServiceMenu?.Name ?? "Unknown",
                Status = displayStatus,
                StatusBadgeColor = new SolidColorBrush(stBg),
                StatusBadgeForeground = new SolidColorBrush(stFg),
                AppointmentSchedule = p.AppointmentDate.HasValue
                    ? p.AppointmentDate.Value.ToString("MMM dd, yyyy") : string.Empty,
                ProcedureSchedule = p.ProcedureDate.HasValue
                    ? p.ProcedureDate.Value.ToString("MMM dd, yyyy") : string.Empty,
                Cost = p.ServiceMenu is not null
                    ? $"₱{p.ServiceMenu.Price:N0}" : string.Empty,
            };
        }

        // ── KPI Cards ──────────────────────────────────────────────────────────
        private void UpdateKpiCards()
        {
            if (TxtTotalProcedures is null) return;

            TxtTotalProcedures.Text = _vm.TotalProcedures.ToString();
            TxtPendingProcedures.Text = _vm.PendingProcedures.ToString();
            TxtScheduledProcedures.Text = _vm.ScheduledProcedures.ToString();
            TxtCompletedProcedures.Text = _vm.CompletedProcedures.ToString();
            TxtRowCount.Text =
                $"Showing {_vm.DisplayedCount} procedure{(_vm.DisplayedCount == 1 ? "" : "s")}";
        }

        // ── Search + Filter ────────────────────────────────────────────────────
        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
            => _vm.SearchText = sender.Text;

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => _vm.SelectedStatus =
                (StatusFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All";

        // ── Schedule Appointment ───────────────────────────────────────────────
        private async void ScheduleAppointment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            var item = _vm.FindProcedure(btn.Tag?.ToString() ?? string.Empty);
            if (item is null) return;

            var dialog = new ScheduleProcedure(
                _procedureController,
                int.Parse(item.ProcedureRecordId),
                item.PatientName,
                item.Initials,
                item.AvatarColor,
                item.ProcedureName)
            { XamlRoot = XamlRoot };

            await dialog.ShowAsync();

            if (!dialog.Confirmed && dialog.SaveError is null) return;

            if (dialog.SaveError is not null)
            {
                ToastHelper.Error(ToastBar, "Failed to schedule appointment", dialog.SaveError.Message);
                return;
            }

            await LoadFromDbAsync();
            ToastHelper.Success(ToastBar, "Appointment scheduled",
                $"{item.ProcedureName} for {item.PatientName} has been scheduled.");
        }
    }
}