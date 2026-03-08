using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace aesth_clic.Views.Roles.Doctor.Modals
{
    // ── Row view-model: medicine list (Step 1) ─────────────────────────────────

    public class MedicineRowItem : INotifyPropertyChanged
    {
        public int MedicineId { get; set; }
        public string MedicineIdTag { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        private bool _selected;
        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                OnPropertyChanged(nameof(RowBackground));
                OnPropertyChanged(nameof(NameWeight));
                OnPropertyChanged(nameof(IsSelected));
                OnPropertyChanged(nameof(CheckBackground));
                if (!value) Quantity = 1;
            }
        }

        private int _quantity = 1;
        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value < 1 ? 1 : value;
                OnPropertyChanged(nameof(QuantityDisplay));
            }
        }

        public string QuantityDisplay => Quantity.ToString();
        public string RowBackground => Selected ? "#EDE4F9" : "Transparent";
        public string NameWeight => Selected ? "SemiBold" : "Normal";
        public Visibility IsSelected => Selected ? Visibility.Visible : Visibility.Collapsed;
        public string CheckBackground => Selected ? "#5B2D8E" : "Transparent";

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // ── Summary row (Step 2) ───────────────────────────────────────────────────

    public class SelectedMedicineSummary
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string QuantityDisplay => $"x{Quantity}";
    }

    // ── Result returned to the caller ──────────────────────────────────────────

    public class MarkDoneResult
    {
        public string ProcedureItemId { get; set; } = string.Empty;
        public List<SelectedMedicineSummary> Medicines { get; set; } = new();
    }

    // ── Modal ──────────────────────────────────────────────────────────────────

    public sealed partial class MarkDoneModal : ContentDialog
    {
        // ── Public outputs ─────────────────────────────────────────────────────
        public MarkDoneResult? Result { get; private set; }
        public Exception? SaveError { get; private set; }

        // ── Procedure context (passed in by caller) ────────────────────────────
        private readonly string _procedureItemId;
        private readonly string _patientName;
        private readonly string _patientGender;
        private readonly string _procedureName;
        private readonly string _appointmentDate;

        // ── Wizard state ───────────────────────────────────────────────────────
        private int _currentStep = 1;

        // ── Medicine data ──────────────────────────────────────────────────────
        private readonly List<MedicineRowItem> _allMedicines;
        private List<MedicineRowItem> _displayedMedicines = new();

        // ── Constructor ────────────────────────────────────────────────────────
        public MarkDoneModal(
            string procedureItemId,
            string patientName,
            string patientGender,
            string procedureName,
            string appointmentDate)
        {
            InitializeComponent();

            _procedureItemId = procedureItemId;
            _patientName = patientName;
            _patientGender = patientGender;
            _procedureName = procedureName;
            _appointmentDate = appointmentDate;

            _allMedicines = BuildMockMedicines();
            RefreshMedicineList(string.Empty);
        }

        // ── Mock medicine data (replace with DB call when ready) ───────────────
        private static List<MedicineRowItem> BuildMockMedicines()
        {
            var names = new[]
            {
                "Amoxicillin 500mg",
                "Ibuprofen 400mg",
                "Paracetamol 500mg",
                "Mefenamic Acid 500mg",
                "Cetirizine 10mg",
                "Metronidazole 500mg",
                "Clindamycin 300mg",
                "Omeprazole 20mg",
                "Betamethasone Cream",
                "Tranexamic Acid 500mg",
            };

            return names.Select((n, i) => new MedicineRowItem
            {
                MedicineId = i + 1,
                MedicineIdTag = (i + 1).ToString(),
                Name = n,
            }).ToList();
        }

        // ── Refresh list with optional search filter ───────────────────────────
        private void RefreshMedicineList(string search)
        {
            _displayedMedicines = string.IsNullOrWhiteSpace(search)
                ? _allMedicines
                : _allMedicines
                    .Where(m => m.Name.ToLower().Contains(search.ToLower()))
                    .ToList();

            MedicineListControl.ItemsSource = null;
            MedicineListControl.ItemsSource = _displayedMedicines;

            UpdateSelectedCount();
        }

        private void UpdateSelectedCount()
        {
            var count = _allMedicines.Count(m => m.Selected);
            TxtSelectedCount.Text = count == 0
                ? "0 selected"
                : $"{count} selected";
        }

        // ── Search ─────────────────────────────────────────────────────────────
        private void MedicineSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
            => RefreshMedicineList(sender.Text);

        // ── Toggle medicine selection ──────────────────────────────────────────
        private void MedicineRow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            if (!int.TryParse(btn.Tag?.ToString(), out int id)) return;

            var medicine = _allMedicines.FirstOrDefault(m => m.MedicineId == id);
            if (medicine is null) return;

            medicine.Selected = !medicine.Selected;
            RefreshMedicineList(MedicineSearchBox.Text);
            ValidationBar.IsOpen = false;
        }

        // ── Quantity steppers ──────────────────────────────────────────────────
        private void IncrementQty_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            if (!int.TryParse(btn.Tag?.ToString(), out int id)) return;

            var medicine = _allMedicines.FirstOrDefault(m => m.MedicineId == id);
            if (medicine is null) return;

            medicine.Quantity++;
            RefreshMedicineList(MedicineSearchBox.Text);
        }

        private void DecrementQty_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            if (!int.TryParse(btn.Tag?.ToString(), out int id)) return;

            var medicine = _allMedicines.FirstOrDefault(m => m.MedicineId == id);
            if (medicine is null) return;

            medicine.Quantity--;                    // setter enforces min = 1
            RefreshMedicineList(MedicineSearchBox.Text);
        }

        // ── Wizard navigation ──────────────────────────────────────────────────
        private void OnPrimaryClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ValidationBar.IsOpen = false;
            args.Cancel = true; // prevent auto-close; we manage it manually

            if (_currentStep == 1)
            {
                var selected = _allMedicines.Where(m => m.Selected).ToList();
                if (selected.Count == 0)
                {
                    ValidationBar.Message = "Please select at least one medicine before continuing.";
                    ValidationBar.IsOpen = true;
                    return;
                }

                GoToStep2(selected);
            }
            else
            {
                _ = ConfirmAndCompleteAsync();
            }
        }

        private void OnCancelClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (_currentStep == 2)
            {
                args.Cancel = true;
                GoToStep1();
            }
            // Step 1: let the dialog close normally (Result stays null)
        }

        // ── Step transitions ───────────────────────────────────────────────────
        private void GoToStep2(List<MedicineRowItem> selected)
        {
            _currentStep = 2;

            Step1Panel.Visibility = Visibility.Collapsed;
            Step2Panel.Visibility = Visibility.Visible;

            // Activate Step 2 indicator
            Step2Circle.Style = (Style)Resources["StepCircleActiveStyle"];
            Step2Number.Foreground = new SolidColorBrush(Colors.White);
            Step2Label.Foreground = new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0x5B, 0x2D, 0x8E));
            ConnectorEnd.Color = ColorHelper.FromArgb(0xFF, 0x5B, 0x2D, 0x8E);

            // Populate recap header
            var parts = _patientName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var initials = parts.Length >= 2
                ? $"{parts[0][0]}{parts[^1][0]}"
                : _patientName.Length > 0 ? _patientName[0].ToString() : "?";

            var avatarColor = _patientGender switch
            {
                "Female" => ColorHelper.FromArgb(0xFF, 0xC2, 0x18, 0x5B),
                "Male" => ColorHelper.FromArgb(0xFF, 0x00, 0x78, 0xD4),
                _ => ColorHelper.FromArgb(0xFF, 0x5B, 0x2D, 0x8E)
            };

            RecapAvatar.Background = new SolidColorBrush(avatarColor);
            RecapInitials.Text = initials.ToUpper();
            RecapPatientName.Text = _patientName;
            RecapProcedureName.Text = _procedureName;
            RecapDate.Text = _appointmentDate;

            // Populate summary medicine list
            var summaries = selected.Select(m => new SelectedMedicineSummary
            {
                Name = m.Name,
                Quantity = m.Quantity,
            }).ToList();

            SummaryListControl.ItemsSource = summaries;

            PrimaryButtonText = "Confirm & Complete";
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

        // ── Confirm & Complete (DB wiring deferred) ────────────────────────────
        private async System.Threading.Tasks.Task ConfirmAndCompleteAsync()
        {
            try
            {
                var selectedMedicines = _allMedicines
                    .Where(m => m.Selected)
                    .Select(m => new SelectedMedicineSummary
                    {
                        Name = m.Name,
                        Quantity = m.Quantity,
                    })
                    .ToList();

                // TODO: wire to backend when ready
                // e.g. await _procedureController.MarkProcedureDoneAsync(_procedureItemId, selectedMedicines);

                Result = new MarkDoneResult
                {
                    ProcedureItemId = _procedureItemId,
                    Medicines = selectedMedicines,
                };

                Hide();
            }
            catch (Exception ex)
            {
                SaveError = ex;
                Hide();
            }
        }
    }
}