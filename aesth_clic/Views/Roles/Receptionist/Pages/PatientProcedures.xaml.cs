using aesth_clic.Tenant.Controller;
using aesth_clic.Tenant.Model;
using aesth_clic.Utils;
using aesth_clic.ViewModels.Receptionist;
using aesth_clic.Views.Roles.Receptionist.Modals;
using aesth_clic.Views.Roles.Receptionist.Modals.PatientProcedure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;

namespace aesth_clic.Views.Roles.Receptionist.Pages
{
    // ── UI display model ───────────────────────────────────────────────────────
    public class PatientProcedureItem
    {
        public string ProcedureRecordId { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string AvatarColor { get; set; } = "#5B2D8E";
        public string ProcedureName { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public string StatusBadgeText { get; set; } = "#F59E0B";

        public string AppointmentSchedule { get; set; } = string.Empty;
        public string ProcedureSchedule { get; set; } = string.Empty;
        public string Cost { get; set; } = string.Empty;

        public Visibility HasAppointmentDate
            => string.IsNullOrEmpty(AppointmentSchedule) ? Visibility.Collapsed : Visibility.Visible;
        public Visibility NoAppointmentDate
            => string.IsNullOrEmpty(AppointmentSchedule) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility HasProcedureDate
            => string.IsNullOrEmpty(ProcedureSchedule) ? Visibility.Collapsed : Visibility.Visible;
        public Visibility NoProcedureDate
            => string.IsNullOrEmpty(ProcedureSchedule) ? Visibility.Visible : Visibility.Collapsed;
    }

    // ── Page ───────────────────────────────────────────────────────────────────
    public sealed partial class PatientProcedures : Page
    {
        private readonly PatientProceduresViewModel _vm = new();

        private readonly PatientProcedureController _procedureController;
        private readonly PatientController _patientController;
        private readonly MenuController _menuController;

        public PatientProcedures()
        {
            InitializeComponent();

            _procedureController = App.Services.GetRequiredService<PatientProcedureController>();
            _patientController = App.Services.GetRequiredService<PatientController>();
            _menuController = App.Services.GetRequiredService<MenuController>();

            ProcedureListControl.ItemsSource = _vm.DisplayedProcedures;

            // Refresh KPI text whenever ViewModel notifies a change
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
                "Female" => "#C2185B",
                "Male" => "#0078D4",
                _ => "#5B2D8E"
            };

            var displayStatus = p.Status?.ToLower() switch
            {
                "pending" => "Pending",
                "scheduled" => "Scheduled",
                "completed" => "Completed",
                "cancelled" => "Cancelled",
                _ => p.Status ?? string.Empty
            };

            var statusColor = displayStatus switch
            {
                "Completed" => "#2E7D32",
                "Scheduled" => "#0078D4",
                "Cancelled" => "#C50F1F",
                _ => "#F59E0B"
            };

            return new PatientProcedureItem
            {
                ProcedureRecordId = p.Id.ToString(),
                PatientId = p.PatientId.ToString(),
                PatientName = fullName,
                Initials = initials.ToUpper(),
                AvatarColor = avatarColor,
                ProcedureName = p.ServiceMenu?.Name ?? "Unknown",
                Status = displayStatus,
                StatusBadgeText = statusColor,
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
        {
            _vm.SearchText = sender.Text;
        }

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _vm.SelectedStatus =
                (StatusFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All";
        }

        // ── Kebab Menu ─────────────────────────────────────────────────────────
        private void KebabMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;

            var recordId = btn.Tag?.ToString() ?? string.Empty;
            var item = _vm.FindProcedure(recordId);
            if (item is null) return;

            var menu = new MenuFlyout();

            // Delete — only for Pending
            var deleteItem = new MenuFlyoutItem
            {
                Text = "Delete",
                Icon = new FontIcon { Glyph = "\uE74D" },
                IsEnabled = item.Status == "Pending",
            };
            deleteItem.Click += async (_, _) =>
            {
                var dialog = new DeleteProcedureConfirmation(
                    item.PatientName, item.Initials, item.AvatarColor,
                    item.ProcedureName, item.Status, item.StatusBadgeText, item.Cost)
                { XamlRoot = XamlRoot };

                await dialog.ShowAsync();
                if (!dialog.Confirmed) return;

                try
                {
                    await _procedureController.DeletePatientProcedureAsync(
                        int.Parse(item.ProcedureRecordId));
                    await LoadFromDbAsync();
                    ToastHelper.Success(ToastBar, "Procedure deleted",
                        $"{item.ProcedureName} for {item.PatientName} has been removed.");
                }
                catch (Exception ex)
                {
                    ToastHelper.Error(ToastBar, "Failed to delete procedure", ex.Message);
                }
            };

            // Cancel — available for any status
            var cancelItem = new MenuFlyoutItem
            {
                Text = "Cancel Procedure",
                Icon = new FontIcon { Glyph = "\uE711" },
            };
            cancelItem.Click += async (_, _) =>
            {
                var dialog = new CancelProcedureConfirmation(
                    item.PatientName, item.Initials, item.AvatarColor,
                    item.ProcedureName, item.Status, item.StatusBadgeText, item.Cost)
                { XamlRoot = XamlRoot };

                await dialog.ShowAsync();
                if (!dialog.Confirmed) return;

                try
                {
                    await _procedureController.DeletePatientProcedureAsync(
                        int.Parse(item.ProcedureRecordId));
                    await LoadFromDbAsync();
                    ToastHelper.Warning(ToastBar, "Procedure cancelled",
                        $"{item.ProcedureName} for {item.PatientName} has been cancelled.");
                }
                catch (Exception ex)
                {
                    ToastHelper.Error(ToastBar, "Failed to cancel procedure", ex.Message);
                }
            };

            menu.Items.Add(deleteItem);
            menu.Items.Add(new MenuFlyoutSeparator());
            menu.Items.Add(cancelItem);
            menu.ShowAt(btn);
        }

        // ── Add Procedure ──────────────────────────────────────────────────────
        private async void AddProcedureButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddPatientProcedure(
                _patientController, _menuController, _procedureController)
            { XamlRoot = XamlRoot };

            await dialog.ShowAsync();

            if (dialog.Result is null && dialog.SaveError is null) return;

            if (dialog.SaveError is not null)
            {
                ToastHelper.Error(ToastBar, "Failed to add procedure", dialog.SaveError.Message);
                return;
            }

            await LoadFromDbAsync();
            ToastHelper.Success(ToastBar, "Procedure added",
                $"{dialog.Result!.Procedure.Name} assigned to {dialog.Result.Patient.FullName}.");
        }
    }
}