using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace aesth_clic.Views.Roles.Pharmacist.Modals
{
    // ── Row view-model ────────────────────────────────────────────────────────

    public class PrescriptionMedicineRow : INotifyPropertyChanged
    {
        public int MedicineId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int PrescribedQty { get; set; }
        public int StockAvailable { get; set; }

        private bool _dispensed;
        public bool Dispensed
        {
            get => _dispensed;
            set
            {
                _dispensed = value;
                OnPropertyChanged(nameof(Dispensed));
                OnPropertyChanged(nameof(RowBackground));
                OnPropertyChanged(nameof(RowOpacity));
                OnPropertyChanged(nameof(DotColor));
                OnPropertyChanged(nameof(DispensingNote));
                OnPropertyChanged(nameof(DispensingNoteVisibility));
                OnPropertyChanged(nameof(UndispensedVisibility));
                OnPropertyChanged(nameof(DispensedVisibility));
            }
        }

        // ── Derived display props ─────────────────────────────────────────────

        public string PrescribedQtyDisplay => $"x{PrescribedQty}";

        public string StockDisplay => $"{StockAvailable} units";

        public string StockStatusLabel => StockAvailable == 0
            ? "Out of stock"
            : StockAvailable < PrescribedQty
                ? "Low stock"
                : "Available";

        public string StockForeground => StockAvailable == 0
            ? "#D32F2F"
            : StockAvailable < PrescribedQty
                ? "#E65100"
                : "#2E7D32";

        public bool CanDispense => !Dispensed && StockAvailable >= PrescribedQty;

        public string RowBackground => Dispensed ? "#F4FBF4" : "Transparent";
        public double RowOpacity => Dispensed ? 0.55 : 1.0;
        public string DotColor => Dispensed ? "#2E7D32" : "#5B2D8E";

        public string DispensingNote => Dispensed ? "Dispensed from inventory" : string.Empty;
        public Visibility DispensingNoteVisibility
            => Dispensed ? Visibility.Visible : Visibility.Collapsed;

        public Visibility UndispensedVisibility
            => Dispensed ? Visibility.Collapsed : Visibility.Visible;
        public Visibility DispensedVisibility
            => Dispensed ? Visibility.Visible : Visibility.Collapsed;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // ── Result returned to caller ─────────────────────────────────────────────

    public class PrescriptionDispenseResult
    {
        public string PatientId { get; set; } = string.Empty;
        public List<PrescriptionMedicineRow> DispensedMedicines { get; set; } = new();
    }

    // ── Modal ─────────────────────────────────────────────────────────────────

    public sealed partial class PatientPrescriptionModal : ContentDialog
    {
        // ── Public outputs ────────────────────────────────────────────────────
        public PrescriptionDispenseResult? Result { get; private set; }

        // ── Context ───────────────────────────────────────────────────────────
        private readonly string _patientId;
        private readonly string _patientName;
        private readonly string _patientGender;
        private readonly string _assignedDoctor;
        private readonly string _procedureName;
        private readonly string _appointmentDate;

        // ── Data ──────────────────────────────────────────────────────────────
        private List<PrescriptionMedicineRow> _medicines = new();

        // ── Constructor ───────────────────────────────────────────────────────
        public PatientPrescriptionModal(
            string patientId,
            string patientName,
            string patientGender,
            string assignedDoctor,
            string procedureName,
            string appointmentDate)
        {
            InitializeComponent();

            _patientId = patientId;
            _patientName = patientName;
            _patientGender = patientGender;
            _assignedDoctor = assignedDoctor;
            _procedureName = procedureName;
            _appointmentDate = appointmentDate;

            PopulateRecapHeader();
            LoadMockPrescription();
            RefreshList();
        }

        // ── Header ────────────────────────────────────────────────────────────
        private void PopulateRecapHeader()
        {
            // Avatar initials
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
            RecapDoctor.Text = _assignedDoctor;
        }

        // ── Mock data (replace with DB/controller call when ready) ────────────
        private void LoadMockPrescription()
        {
            // TODO: replace with real prescription loaded by _patientId
            _medicines = new List<PrescriptionMedicineRow>
            {
                new() { MedicineId = 1, Name = "Amoxicillin 500mg",   PrescribedQty = 2, StockAvailable = 48 },
                new() { MedicineId = 2, Name = "Ibuprofen 400mg",      PrescribedQty = 3, StockAvailable = 12 },
                new() { MedicineId = 3, Name = "Paracetamol 500mg",    PrescribedQty = 4, StockAvailable = 2  }, // low stock — only 2 of 4 required
                new() { MedicineId = 4, Name = "Mefenamic Acid 500mg", PrescribedQty = 2, StockAvailable = 0  }, // out of stock
                new() { MedicineId = 5, Name = "Cetirizine 10mg",      PrescribedQty = 1, StockAvailable = 30 },
            };
        }

        // ── List refresh ──────────────────────────────────────────────────────
        private void RefreshList()
        {
            PrescriptionListControl.ItemsSource = null;
            PrescriptionListControl.ItemsSource = _medicines;
            UpdateDispensedCount();
            UpdateMarkCompleteButton();
            UpdateDispenseAllButton();
        }

        private void UpdateDispensedCount()
        {
            var dispensed = _medicines.Count(m => m.Dispensed);
            var total = _medicines.Count;
            TxtDispensedCount.Text = $"{dispensed} / {total} dispensed";
        }

        private void UpdateMarkCompleteButton()
        {
            // Enabled only when every medicine has been dispensed.
            // Out-of-stock and low-stock rows permanently block this until stock is resolved.
            IsPrimaryButtonEnabled = _medicines.Count > 0 && _medicines.All(m => m.Dispensed);
        }

        private void UpdateDispenseAllButton()
        {
            if (BtnDispenseAll is null) return;
            BtnDispenseAll.IsEnabled = _medicines.Any(m => !m.Dispensed && m.CanDispense);
        }

        // ── Per-row dispense ──────────────────────────────────────────────────
        private void DispenseRow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            if (!int.TryParse(btn.Tag?.ToString(), out int id)) return;

            var row = _medicines.FirstOrDefault(m => m.MedicineId == id);
            if (row is null || row.Dispensed) return;

            row.Dispensed = true;
            ValidationBar.IsOpen = false;
            RefreshList();
        }

        // ── Dispense All (header button) ──────────────────────────────────────
        private void OnDispenseAllClicked(object sender, RoutedEventArgs e)
        {
            var skipped = _medicines.Where(m => !m.Dispensed && !m.CanDispense).ToList();
            var pending = _medicines.Where(m => !m.Dispensed && m.CanDispense).ToList();

            if (pending.Count == 0)
            {
                ValidationBar.Message = "Remaining medicines cannot be dispensed due to insufficient stock.";
                ValidationBar.Severity = InfoBarSeverity.Warning;
                ValidationBar.IsOpen = true;
                return;
            }

            foreach (var row in pending)
                row.Dispensed = true;

            ValidationBar.IsOpen = false;
            RefreshList();

            if (skipped.Count > 0)
            {
                ValidationBar.Message = $"{skipped.Count} medicine(s) could not be dispensed due to insufficient stock.";
                ValidationBar.Severity = InfoBarSeverity.Warning;
                ValidationBar.IsOpen = true;
            }
        }

        // ── Mark Complete (primary footer button) ─────────────────────────────
        private void OnMarkCompleteClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true; // manage close manually

            Result = new PrescriptionDispenseResult
            {
                PatientId = _patientId,
                DispensedMedicines = _medicines.Where(m => m.Dispensed).ToList(),
            };

            // TODO: persist to backend here
            // e.g. await _pharmacistController.CompletePrescriptionAsync(_patientId, Result.DispensedMedicines);

            Hide();
        }
    }
}