using aesth_clic.Tenant.Controller;
using aesth_clic.Tenant.Model;
using aesth_clic.Utils;
using aesth_clic.Views.Roles.Receptionist.Modals;
using aesth_clic.Views.Roles.Receptionist.Modals.PatientProcedure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;

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
        private List<PatientProcedureItem> _allProcedures = new();

        private readonly PatientProcedureController _procedureController;
        private readonly PatientController _patientController;
        private readonly MenuController _menuController;

        public PatientProcedures()
        {
            InitializeComponent();

            _procedureController = App.Services.GetRequiredService<PatientProcedureController>();
            _patientController = App.Services.GetRequiredService<PatientController>();
            _menuController = App.Services.GetRequiredService<MenuController>();

            _ = LoadFromDbAsync();
        }

        // ── Data loading ───────────────────────────────────────────────────────
        private async System.Threading.Tasks.Task LoadFromDbAsync()
        {
            try
            {
                var records = await _procedureController.GetAllPatientProceduresAsync();

                _allProcedures = records.Select(MapToItem).ToList();

                ApplyFilters();
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

            // Normalize DB lowercase status → Title case for display
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
                _ => "#F59E0B"   // Pending
            };

            var appointmentSchedule = p.AppointmentDate.HasValue
                ? p.AppointmentDate.Value.ToString("MMM dd, yyyy")
                : string.Empty;

            var procedureSchedule = p.ProcedureDate.HasValue
                ? p.ProcedureDate.Value.ToString("MMM dd, yyyy")
                : string.Empty;

            var cost = p.ServiceMenu is not null
                ? $"₱{p.ServiceMenu.Price:N0}"
                : string.Empty;

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
                AppointmentSchedule = appointmentSchedule,
                ProcedureSchedule = procedureSchedule,
                Cost = cost,
            };
        }

        // ── Filtering ──────────────────────────────────────────────────────────
        private void ApplyFilters()
        {
            if (ProcedureListControl is null) return;

            var search = SearchBox?.Text?.Trim().ToLower() ?? string.Empty;
            var statusTag = (StatusFilter?.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All";

            var filtered = _allProcedures.Where(p =>
                (string.IsNullOrEmpty(search)
                 || p.PatientName.ToLower().Contains(search)
                 || p.ProcedureName.ToLower().Contains(search))
                && (statusTag == "All" || p.Status == statusTag)
            ).ToList();

            ProcedureListControl.ItemsSource = filtered;

            // KPI cards always reflect full unfiltered list
            var total = _allProcedures.Count;
            var pending = _allProcedures.Count(p => p.Status == "Pending");
            var scheduled = _allProcedures.Count(p => p.Status == "Scheduled");
            var completed = _allProcedures.Count(p => p.Status == "Completed");

            if (TxtTotalProcedures is not null) TxtTotalProcedures.Text = total.ToString();
            if (TxtPendingProcedures is not null) TxtPendingProcedures.Text = pending.ToString();
            if (TxtScheduledProcedures is not null) TxtScheduledProcedures.Text = scheduled.ToString();
            if (TxtCompletedProcedures is not null) TxtCompletedProcedures.Text = completed.ToString();
            if (TxtRowCount is not null)
                TxtRowCount.Text = $"Showing {filtered.Count} procedure{(filtered.Count == 1 ? "" : "s")}";
        }

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
            => ApplyFilters();

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => ApplyFilters();

        // ── Kebab Menu ─────────────────────────────────────────────────────────
        private void KebabMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;

            var recordId = btn.Tag?.ToString() ?? string.Empty;
            var item = _allProcedures.FirstOrDefault(p => p.ProcedureRecordId == recordId);
            if (item is null) return;

            var menu = new MenuFlyout();

            // ── Delete — only available for Pending ────────────────────────
            var deleteItem = new MenuFlyoutItem
            {
                Text = "Delete",
                Icon = new FontIcon { Glyph = "\uE74D" },
                IsEnabled = item.Status == "Pending",
            };
            deleteItem.Click += async (_, _) =>
            {
                var dialog = new DeleteProcedureConfirmation(
                    patientName: item.PatientName,
                    initials: item.Initials,
                    avatarColor: item.AvatarColor,
                    procedureName: item.ProcedureName,
                    status: item.Status,
                    statusColor: item.StatusBadgeText,
                    cost: item.Cost)
                { XamlRoot = XamlRoot };

                await dialog.ShowAsync();
                if (!dialog.Confirmed) return;

                try
                {
                    await _procedureController.DeletePatientProcedureAsync(
                        int.Parse(item.ProcedureRecordId));

                    await LoadFromDbAsync();

                    ToastHelper.Success(
                        ToastBar,
                        "Procedure deleted",
                        $"{item.ProcedureName} for {item.PatientName} has been removed.");
                }
                catch (Exception ex)
                {
                    ToastHelper.Error(ToastBar, "Failed to delete procedure", ex.Message);
                }
            };

            // ── Cancel — hard delete, available for any status ─────────────
            var cancelItem = new MenuFlyoutItem
            {
                Text = "Cancel Procedure",
                Icon = new FontIcon { Glyph = "\uE711" },
            };
            cancelItem.Click += async (_, _) =>
            {
                var dialog = new CancelProcedureConfirmation(
                    patientName: item.PatientName,
                    initials: item.Initials,
                    avatarColor: item.AvatarColor,
                    procedureName: item.ProcedureName,
                    status: item.Status,
                    statusColor: item.StatusBadgeText,
                    cost: item.Cost)
                { XamlRoot = XamlRoot };

                await dialog.ShowAsync();
                if (!dialog.Confirmed) return;

                try
                {
                    await _procedureController.DeletePatientProcedureAsync(
                        int.Parse(item.ProcedureRecordId));

                    await LoadFromDbAsync();

                    ToastHelper.Warning(
                        ToastBar,
                        "Procedure cancelled",
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
                _patientController,
                _menuController,
                _procedureController)
            { XamlRoot = XamlRoot };

            await dialog.ShowAsync();

            // User cancelled the wizard — do nothing
            if (dialog.Result is null && dialog.SaveError is null) return;

            if (dialog.SaveError is not null)
            {
                ToastHelper.Error(ToastBar, "Failed to add procedure", dialog.SaveError.Message);
                return;
            }

            await LoadFromDbAsync();

            ToastHelper.Success(
                ToastBar,
                "Procedure added",
                $"{dialog.Result!.Procedure.Name} assigned to {dialog.Result.Patient.FullName}.");
        }
    }
}