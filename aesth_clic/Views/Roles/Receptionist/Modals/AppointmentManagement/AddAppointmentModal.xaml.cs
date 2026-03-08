using aesth_clic.Tenant.Controller;
using aesth_clic.Tenant.Dto.PatientProcedure;
using aesth_clic.Tenant.Model;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;

// Alias to disambiguate from the Dto.PatientProcedure namespace
using PatientProcedureModel = aesth_clic.Tenant.Model.PatientProcedure;

namespace aesth_clic.Views.Roles.Receptionist.Modals
{
    // ── Row view-models ────────────────────────────────────────────────────────

    public class AppointmentProcedureRowItem
    {
        public int ProcedureRecordId { get; set; }
        public string ProcedureRecordIdTag { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string AvatarColor { get; set; } = "#5B2D8E";
        public string ProcedureName { get; set; } = string.Empty;

        private bool _selected;
        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                RowBackground = value ? "#EDE4F9" : "Transparent";
                NameWeight = value ? "SemiBold" : "Normal";
                IsSelected = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public string RowBackground { get; private set; } = "Transparent";
        public string NameWeight { get; private set; } = "Normal";
        public Visibility IsSelected { get; private set; } = Visibility.Collapsed;
    }

    public class AppointmentDoctorRowItem
    {
        public int DoctorId { get; set; }
        public string DoctorIdTag { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;

        private bool _selected;
        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                RowBackground = value ? "#EDE4F9" : "Transparent";
                NameWeight = value ? "SemiBold" : "Normal";
                IsSelected = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public string RowBackground { get; private set; } = "Transparent";
        public string NameWeight { get; private set; } = "Normal";
        public Visibility IsSelected { get; private set; } = Visibility.Collapsed;
    }

    // ── Result ─────────────────────────────────────────────────────────────────

    public class AppointmentResult
    {
        public string PatientName { get; set; } = string.Empty;
        public string ProcedureName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
    }

    // ── Modal ──────────────────────────────────────────────────────────────────

    public sealed partial class AddAppointmentModal : ContentDialog
    {
        // ── Public result ──────────────────────────────────────────────────────
        public AppointmentResult? Result { get; private set; }
        public Exception? SaveError { get; private set; }

        // ── Bound to XAML (MinDate for CalendarDatePicker) ─────────────────────
        public DateTimeOffset TodayDate { get; } = DateTimeOffset.Now.Date;

        // ── State ──────────────────────────────────────────────────────────────
        private int _currentStep = 1;
        private readonly bool _isEditMode;

        private readonly PatientProcedureController _procedureController;
        private readonly UserController _userController;

        private List<PatientProcedureModel> _allPendingProcedures = new();
        private List<User> _allDoctors = new();

        private List<AppointmentProcedureRowItem> _procedureRows = new();
        private List<AppointmentDoctorRowItem> _doctorRows = new();

        private int _selectedProcedureRecordId = -1;
        private int _selectedDoctorId = -1;

        // ── Constructors ───────────────────────────────────────────────────────

        /// <summary>Add mode</summary>
        public AddAppointmentModal(
            PatientProcedureController procedureController,
            UserController userController)
        {
            _procedureController = procedureController;
            _userController = userController;
            _isEditMode = false;

            InitializeComponent();
            Title = "Add Appointment";
            _ = LoadDataAsync();
        }

        /// <summary>Edit mode — pre-selects procedure, doctor, and date.</summary>
        public AddAppointmentModal(
            PatientProcedureController procedureController,
            UserController userController,
            int existingProcedureRecordId,
            int existingDoctorId,
            DateTime existingDate)
        {
            _procedureController = procedureController;
            _userController = userController;
            _isEditMode = true;
            _selectedProcedureRecordId = existingProcedureRecordId;
            _selectedDoctorId = existingDoctorId;

            InitializeComponent();
            Title = "Edit Appointment";

            // Pre-fill date picker after XAML is ready
            AppointmentDatePicker.Date = new DateTimeOffset(existingDate.Date, TimeSpan.Zero);

            _ = LoadDataAsync();
        }

        // ── Data loading ───────────────────────────────────────────────────────
        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                var allProcedures = await _procedureController.GetAllPatientProceduresAsync();
                _allPendingProcedures = allProcedures
                    .Where(p => p.Status.Equals("pending", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                _allDoctors = await _userController.GetAvailableDoctorsAsync();

                BuildProcedureRows();
                BuildDoctorRows();
                RefreshProcedureList(string.Empty);
                RefreshDoctorList(string.Empty);
            }
            catch (Exception ex)
            {
                ValidationBar.Message = $"Failed to load data: {ex.Message}";
                ValidationBar.IsOpen = true;
            }
        }

        // ── Build row view-models ──────────────────────────────────────────────
        private void BuildProcedureRows()
        {
            _procedureRows = _allPendingProcedures.Select(p =>
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

                return new AppointmentProcedureRowItem
                {
                    ProcedureRecordId = p.Id,
                    ProcedureRecordIdTag = p.Id.ToString(),
                    PatientName = fullName,
                    Initials = initials.ToUpper(),
                    AvatarColor = avatarColor,
                    ProcedureName = p.ServiceMenu?.Name ?? "Unknown",
                };
            }).ToList();
        }

        private void BuildDoctorRows()
        {
            _doctorRows = _allDoctors.Select(d =>
            {
                var parts = d.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var initials = parts.Length >= 2
                    ? $"{parts[0][0]}{parts[^1][0]}"
                    : d.FullName.Length > 0 ? d.FullName[0].ToString() : "?";

                return new AppointmentDoctorRowItem
                {
                    DoctorId = d.Id,
                    DoctorIdTag = d.Id.ToString(),
                    FullName = d.FullName,
                    Initials = initials.ToUpper(),
                };
            }).ToList();
        }

        // ── Refresh helpers ────────────────────────────────────────────────────
        private void RefreshProcedureList(string search)
        {
            var filtered = string.IsNullOrWhiteSpace(search)
                ? _procedureRows
                : _procedureRows.Where(r =>
                    r.PatientName.ToLower().Contains(search.ToLower()) ||
                    r.ProcedureName.ToLower().Contains(search.ToLower())).ToList();

            foreach (var row in filtered)
                row.Selected = row.ProcedureRecordId == _selectedProcedureRecordId;

            ProcedureListControl.ItemsSource = null;
            ProcedureListControl.ItemsSource = filtered;
            TxtProcedureCount.Text = filtered.Count.ToString();
        }

        private void RefreshDoctorList(string search)
        {
            var filtered = string.IsNullOrWhiteSpace(search)
                ? _doctorRows
                : _doctorRows.Where(r =>
                    r.FullName.ToLower().Contains(search.ToLower())).ToList();

            foreach (var row in filtered)
                row.Selected = row.DoctorId == _selectedDoctorId;

            DoctorListControl.ItemsSource = null;
            DoctorListControl.ItemsSource = filtered;
            TxtDoctorCount.Text = filtered.Count.ToString();
        }

        // ── Search handlers ────────────────────────────────────────────────────
        private void ProcedureSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
            => RefreshProcedureList(sender.Text);

        private void DoctorSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
            => RefreshDoctorList(sender.Text);

        // ── Row click handlers ─────────────────────────────────────────────────
        private void ProcedureRow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            if (!int.TryParse(btn.Tag?.ToString(), out int id)) return;

            _selectedProcedureRecordId = id;
            RefreshProcedureList(ProcedureSearchBox.Text);
            ValidationBar.IsOpen = false;
        }

        private void DoctorRow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            if (!int.TryParse(btn.Tag?.ToString(), out int id)) return;

            _selectedDoctorId = id;
            RefreshDoctorList(DoctorSearchBox.Text);
            ValidationBar.IsOpen = false;
        }

        // ── Wizard navigation ──────────────────────────────────────────────────
        private void OnPrimaryClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ValidationBar.IsOpen = false;
            args.Cancel = true; // always prevent auto-close; we manage it manually

            switch (_currentStep)
            {
                case 1:
                    if (_selectedProcedureRecordId == -1)
                    {
                        ValidationBar.Message = "Please select a patient procedure before continuing.";
                        ValidationBar.IsOpen = true;
                        return;
                    }
                    GoToStep2();
                    break;

                case 2:
                    if (_selectedDoctorId == -1)
                    {
                        ValidationBar.Message = "Please select a doctor before continuing.";
                        ValidationBar.IsOpen = true;
                        return;
                    }
                    GoToStep3();
                    break;

                case 3:
                    if (AppointmentDatePicker.Date is null)
                    {
                        ValidationBar.Message = "Please select an appointment date.";
                        ValidationBar.IsOpen = true;
                        return;
                    }
                    _ = ConfirmAndSaveAsync();
                    break;
            }
        }

        private void OnCancelClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (_currentStep == 2)
            {
                args.Cancel = true;
                GoToStep1();
            }
            else if (_currentStep == 3)
            {
                args.Cancel = true;
                GoToStep2FromStep3();
            }
            // step 1: let dialog close normally
        }

        private async System.Threading.Tasks.Task ConfirmAndSaveAsync()
        {
            try
            {
                var date = AppointmentDatePicker.Date!.Value.DateTime;

                var dto = new SchedulePatientProcedureDto
                {
                    PatientProcedureId = _selectedProcedureRecordId,
                    AssignedDoctorId = _selectedDoctorId,
                    AppointmentDate = date,
                };

                await _procedureController.ScheduleProcedureAsync(dto);

                var procedure = _allPendingProcedures.FirstOrDefault(p => p.Id == _selectedProcedureRecordId);
                var doctor = _allDoctors.FirstOrDefault(d => d.Id == _selectedDoctorId);

                Result = new AppointmentResult
                {
                    PatientName = procedure?.Patient?.FullName ?? "Unknown",
                    ProcedureName = procedure?.ServiceMenu?.Name ?? "Unknown",
                    DoctorName = doctor?.FullName ?? "Unknown",
                    AppointmentDate = date,
                };

                Hide();
            }
            catch (Exception ex)
            {
                SaveError = ex;
                Hide();
            }
        }

        // ── Step transitions ───────────────────────────────────────────────────
        private void GoToStep2()
        {
            _currentStep = 2;

            Step1Panel.Visibility = Visibility.Collapsed;
            Step2Panel.Visibility = Visibility.Visible;
            Step3Panel.Visibility = Visibility.Collapsed;

            // Step indicator
            Step2Circle.Style = (Style)Resources["StepCircleActiveStyle"];
            Step2Number.Foreground = new SolidColorBrush(Colors.White);
            Step2Label.Foreground = new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0x5B, 0x2D, 0x8E));
            Connector1End.Color = ColorHelper.FromArgb(0xFF, 0x5B, 0x2D, 0x8E);

            // Populate recap
            var procedure = _allPendingProcedures.FirstOrDefault(p => p.Id == _selectedProcedureRecordId);
            if (procedure is not null)
            {
                var fullName = procedure.Patient?.FullName ?? "Unknown";
                var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var initials = parts.Length >= 2
                    ? $"{parts[0][0]}{parts[^1][0]}"
                    : fullName.Length > 0 ? fullName[0].ToString() : "?";

                var gender = procedure.Patient?.Gender ?? string.Empty;
                var avatarColor = gender switch
                {
                    "Female" => ColorHelper.FromArgb(0xFF, 0xC2, 0x18, 0x5B),
                    "Male" => ColorHelper.FromArgb(0xFF, 0x00, 0x78, 0xD4),
                    _ => ColorHelper.FromArgb(0xFF, 0x5B, 0x2D, 0x8E)
                };

                RecapStep2Avatar.Background = new SolidColorBrush(avatarColor);
                RecapStep2Initials.Text = initials.ToUpper();
                RecapStep2PatientName.Text = fullName;
                RecapStep2ProcedureName.Text = procedure.ServiceMenu?.Name ?? "Unknown";
            }

            PrimaryButtonText = "Next";
            CloseButtonText = "← Back";
        }

        private void GoToStep3()
        {
            _currentStep = 3;

            Step1Panel.Visibility = Visibility.Collapsed;
            Step2Panel.Visibility = Visibility.Collapsed;
            Step3Panel.Visibility = Visibility.Visible;

            // Step indicator
            Step3Circle.Style = (Style)Resources["StepCircleActiveStyle"];
            Step3Number.Foreground = new SolidColorBrush(Colors.White);
            Step3Label.Foreground = new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0x5B, 0x2D, 0x8E));
            Connector2Start.Color = ColorHelper.FromArgb(0xFF, 0x5B, 0x2D, 0x8E);
            Connector2End.Color = ColorHelper.FromArgb(0xFF, 0x5B, 0x2D, 0x8E);

            // Populate recap
            var procedure = _allPendingProcedures.FirstOrDefault(p => p.Id == _selectedProcedureRecordId);
            var doctor = _allDoctors.FirstOrDefault(d => d.Id == _selectedDoctorId);

            if (procedure is not null)
            {
                var fullName = procedure.Patient?.FullName ?? "Unknown";
                var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var initials = parts.Length >= 2
                    ? $"{parts[0][0]}{parts[^1][0]}"
                    : fullName.Length > 0 ? fullName[0].ToString() : "?";

                var gender = procedure.Patient?.Gender ?? string.Empty;
                var avatarColor = gender switch
                {
                    "Female" => ColorHelper.FromArgb(0xFF, 0xC2, 0x18, 0x5B),
                    "Male" => ColorHelper.FromArgb(0xFF, 0x00, 0x78, 0xD4),
                    _ => ColorHelper.FromArgb(0xFF, 0x5B, 0x2D, 0x8E)
                };

                RecapStep3Avatar.Background = new SolidColorBrush(avatarColor);
                RecapStep3Initials.Text = initials.ToUpper();
                RecapStep3PatientName.Text = fullName;
                RecapStep3ProcedureName.Text = procedure.ServiceMenu?.Name ?? "Unknown";
            }

            if (doctor is not null)
            {
                var parts = doctor.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var initials = parts.Length >= 2
                    ? $"{parts[0][0]}{parts[^1][0]}"
                    : doctor.FullName.Length > 0 ? doctor.FullName[0].ToString() : "?";

                RecapStep3DoctorInitials.Text = initials.ToUpper();
                RecapStep3DoctorName.Text = doctor.FullName;
            }

            PrimaryButtonText = _isEditMode ? "Save Changes" : "Schedule Appointment";
            CloseButtonText = "← Back";
        }

        private void GoToStep1()
        {
            _currentStep = 1;

            Step1Panel.Visibility = Visibility.Visible;
            Step2Panel.Visibility = Visibility.Collapsed;
            Step3Panel.Visibility = Visibility.Collapsed;

            // Step indicator — reset step 2
            Step2Circle.Style = (Style)Resources["StepCircleInactiveStyle"];
            Step2Number.Foreground = new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0x9B, 0x80, 0xC4));
            Step2Label.Foreground = new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0x9B, 0x80, 0xC4));
            Connector1End.Color = ColorHelper.FromArgb(0xFF, 0xE4, 0xDA, 0xF5);

            PrimaryButtonText = "Next";
            CloseButtonText = "Cancel";
        }

        private void GoToStep2FromStep3()
        {
            _currentStep = 2;

            Step1Panel.Visibility = Visibility.Collapsed;
            Step2Panel.Visibility = Visibility.Visible;
            Step3Panel.Visibility = Visibility.Collapsed;

            // Step indicator — reset step 3
            Step3Circle.Style = (Style)Resources["StepCircleInactiveStyle"];
            Step3Number.Foreground = new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0x9B, 0x80, 0xC4));
            Step3Label.Foreground = new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0x9B, 0x80, 0xC4));
            Connector2Start.Color = ColorHelper.FromArgb(0xFF, 0xE4, 0xDA, 0xF5);
            Connector2End.Color = ColorHelper.FromArgb(0xFF, 0xE4, 0xDA, 0xF5);

            PrimaryButtonText = "Next";
            CloseButtonText = "← Back";
        }
    }
}