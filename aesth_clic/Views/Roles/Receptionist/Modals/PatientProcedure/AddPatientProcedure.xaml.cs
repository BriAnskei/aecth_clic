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

namespace aesth_clic.Views.Roles.Receptionist.Modals
{
    // ── Row view-models ────────────────────────────────────────────────────────

    public class PatientRowItem
    {
        public int PatientId { get; set; }
        public string PatientIdTag { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string AvatarColor { get; set; } = "#5B2D8E";
        public string Gender { get; set; } = string.Empty;
        public string AgeDisplay { get; set; } = string.Empty;

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
                NotSelected = value ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public string RowBackground { get; private set; } = "Transparent";
        public string NameWeight { get; private set; } = "Normal";
        public Visibility IsSelected { get; private set; } = Visibility.Collapsed;
        public Visibility NotSelected { get; private set; } = Visibility.Visible;
    }

    public class ProcedureRowItem
    {
        public int ProcedureId { get; set; }
        public string ProcedureIdTag { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string PriceDisplay { get; set; } = string.Empty;
        public double Price { get; set; }

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

    // ── Result DTO ─────────────────────────────────────────────────────────────

    public class ProcedureResult
    {
        public Patient Patient { get; set; } = new();
        public ServiceMenu Procedure { get; set; } = new();
        public string Status { get; set; } = "Pending";
    }

    // ── Modal ──────────────────────────────────────────────────────────────────

    public sealed partial class AddPatientProcedure : ContentDialog
    {
        public ProcedureResult? Result { get; private set; }
        public Exception? SaveError { get; private set; }

        private int _currentStep = 1;

        private readonly PatientController _patientController;
        private readonly MenuController _menuController;
        private readonly PatientProcedureController _procedureController;

        private List<Patient> _allPatients = new();
        private List<ServiceMenu> _allServices = new();

        private List<PatientRowItem> _patientRows = new();
        private List<ProcedureRowItem> _procedureRows = new();

        private int _selectedPatientId = -1;
        private int _selectedProcedureId = -1;

        // ── Constructor ────────────────────────────────────────────────────────
        public AddPatientProcedure(
            PatientController patientController,
            MenuController menuController,
            PatientProcedureController procedureController)
        {
            InitializeComponent();

            _patientController = patientController;
            _menuController = menuController;
            _procedureController = procedureController;

            // Load data from DB on opening
            _ = LoadDataAsync();
        }

        // ── Load real data from DB ─────────────────────────────────────────────
        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                _allPatients = await _patientController.GetAllPatientsAsync();
                _allServices = await _menuController.GetAllServicesAsync();

                BuildPatientRows();
                BuildProcedureRows();
                RefreshPatientList(string.Empty);
                RefreshProcedureList(string.Empty);
            }
            catch (Exception ex)
            {
                ValidationBar.Message = $"Failed to load data: {ex.Message}";
                ValidationBar.IsOpen = true;
            }
        }

        // ── Build row view-models ──────────────────────────────────────────────
        private void BuildPatientRows()
        {
            _patientRows = _allPatients.Select(p =>
            {
                var parts = p.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var initials = parts.Length >= 2
                    ? $"{parts[0][0]}{parts[^1][0]}"
                    : p.FullName.Length > 0 ? p.FullName[0].ToString() : "?";

                var avatarColor = p.Gender switch
                {
                    "Female" => "#C2185B",
                    "Male" => "#0078D4",
                    _ => "#5B2D8E"
                };

                return new PatientRowItem
                {
                    PatientId = p.Id,
                    PatientIdTag = p.Id.ToString(),
                    FullName = p.FullName,
                    Initials = initials.ToUpper(),
                    AvatarColor = avatarColor,
                    Gender = p.Gender,
                    AgeDisplay = p.Age.ToString(),
                };
            }).ToList();
        }

        private void BuildProcedureRows()
        {
            _procedureRows = _allServices.Select(s => new ProcedureRowItem
            {
                ProcedureId = s.Id,
                ProcedureIdTag = s.Id.ToString(),
                Name = s.Name,
                Price = s.Price,
                PriceDisplay = $"₱{s.Price:N0}",
            }).ToList();
        }

        // ── Refresh helpers ────────────────────────────────────────────────────
        private void RefreshPatientList(string search)
        {
            var filtered = string.IsNullOrWhiteSpace(search)
                ? _patientRows
                : _patientRows.Where(r =>
                    r.FullName.ToLower().Contains(search.ToLower()) ||
                    r.Gender.ToLower().Contains(search.ToLower())).ToList();

            foreach (var row in filtered)
                row.Selected = row.PatientId == _selectedPatientId;

            PatientListControl.ItemsSource = null;
            PatientListControl.ItemsSource = filtered;
            TxtPatientCount.Text = filtered.Count.ToString();
        }

        private void RefreshProcedureList(string search)
        {
            var filtered = string.IsNullOrWhiteSpace(search)
                ? _procedureRows
                : _procedureRows.Where(r =>
                    r.Name.ToLower().Contains(search.ToLower())).ToList();

            foreach (var row in filtered)
                row.Selected = row.ProcedureId == _selectedProcedureId;

            ProcedureListControl.ItemsSource = null;
            ProcedureListControl.ItemsSource = filtered;
            TxtProcedureCount.Text = filtered.Count.ToString();
        }

        // ── Search handlers ────────────────────────────────────────────────────
        private void PatientSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
            => RefreshPatientList(sender.Text);

        private void ProcedureSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
            => RefreshProcedureList(sender.Text);

        // ── Row click handlers ─────────────────────────────────────────────────
        private void PatientRow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            if (!int.TryParse(btn.Tag?.ToString(), out int patientId)) return;

            _selectedPatientId = patientId;
            RefreshPatientList(PatientSearchBox.Text);
            ValidationBar.IsOpen = false;
        }

        private void ProcedureRow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            if (!int.TryParse(btn.Tag?.ToString(), out int procedureId)) return;

            _selectedProcedureId = procedureId;
            RefreshProcedureList(ProcedureSearchBox.Text);
            ValidationBar.IsOpen = false;
        }

        // ── Wizard navigation ──────────────────────────────────────────────────
        private void OnPrimaryClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ValidationBar.IsOpen = false;
            args.Cancel = true; // always prevent auto-close; we manage it manually

            if (_currentStep == 1)
            {
                if (_selectedPatientId == -1)
                {
                    ValidationBar.Message = "Please select a patient before continuing.";
                    ValidationBar.IsOpen = true;
                    return;
                }

                GoToStep2();
            }
            else
            {
                if (_selectedProcedureId == -1)
                {
                    ValidationBar.Message = "Please select a procedure before adding.";
                    ValidationBar.IsOpen = true;
                    return;
                }

                _ = ConfirmAndSaveAsync();
            }
        }

        private async System.Threading.Tasks.Task ConfirmAndSaveAsync()
        {
            try
            {
                var dto = new NewPatientProcedureDto
                {
                    PatientId = _selectedPatientId,
                    ProcedureId = _selectedProcedureId,
                };

                await _procedureController.AddPatientProcedureAsync(dto);

                // Build Result so the page can compose its toast message
                var patient = _allPatients.FirstOrDefault(p => p.Id == _selectedPatientId)!;
                var procedure = _allServices.FirstOrDefault(s => s.Id == _selectedProcedureId)!;

                Result = new ProcedureResult
                {
                    Patient = patient,
                    Procedure = procedure,
                    Status = "Pending",
                };

                Hide();
            }
            catch (Exception ex)
            {
                SaveError = ex;
                Hide();
            }
        }

        private void OnCancelClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (_currentStep == 2)
            {
                args.Cancel = true;
                GoToStep1();
            }
            // step 1: let the dialog close normally (Result stays null)
        }

        // ── Step transitions ───────────────────────────────────────────────────
        private void GoToStep2()
        {
            _currentStep = 2;

            Step1Panel.Visibility = Visibility.Collapsed;
            Step2Panel.Visibility = Visibility.Visible;

            Step2Circle.Style = (Style)Resources["StepCircleActiveStyle"];
            Step2Number.Foreground = new SolidColorBrush(Colors.White);
            Step2Label.Foreground = new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0x5B, 0x2D, 0x8E));
            ConnectorEnd.Color = ColorHelper.FromArgb(0xFF, 0x5B, 0x2D, 0x8E);

            var patient = _allPatients.FirstOrDefault(p => p.Id == _selectedPatientId);
            if (patient is not null)
            {
                var parts = patient.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var initials = parts.Length >= 2
                    ? $"{parts[0][0]}{parts[^1][0]}"
                    : patient.FullName.Length > 0 ? patient.FullName[0].ToString() : "?";

                var avatarColor = patient.Gender switch
                {
                    "Female" => ColorHelper.FromArgb(0xFF, 0xC2, 0x18, 0x5B),
                    "Male" => ColorHelper.FromArgb(0xFF, 0x00, 0x78, 0xD4),
                    _ => ColorHelper.FromArgb(0xFF, 0x5B, 0x2D, 0x8E)
                };

                RecapAvatar.Background = new SolidColorBrush(avatarColor);
                RecapInitials.Text = initials.ToUpper();
                RecapPatientName.Text = patient.FullName;
                RecapPatientSub.Text = $"{patient.Gender} · Age {patient.Age} · {patient.PhoneNumber}";
            }

            PrimaryButtonText = "Add Procedure";
            CloseButtonText = "← Back";
        }

        private void GoToStep1()
        {
            _currentStep = 1;

            Step2Panel.Visibility = Visibility.Collapsed;
            Step1Panel.Visibility = Visibility.Visible;

            Step2Circle.Style = (Style)Resources["StepCircleInactiveStyle"];
            Step2Number.Foreground = new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0x9B, 0x80, 0xC4));
            Step2Label.Foreground = new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0x9B, 0x80, 0xC4));
            ConnectorEnd.Color = ColorHelper.FromArgb(0xFF, 0xE4, 0xDA, 0xF5);

            PrimaryButtonText = "Next";
            CloseButtonText = "Cancel";
        }
    }
}