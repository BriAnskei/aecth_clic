using aesth_clic.Tenant.Controller;
using aesth_clic.Tenant.Model;
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
    // ── Row view-model ─────────────────────────────────────────────────────────────
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

        // ── Derived display props ──────────────────────────────────────────────────
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
        public Visibility DispensingNoteVisibility => Dispensed ? Visibility.Visible : Visibility.Collapsed;
        public Visibility UndispensedVisibility => Dispensed ? Visibility.Collapsed : Visibility.Visible;
        public Visibility DispensedVisibility => Dispensed ? Visibility.Visible : Visibility.Collapsed;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // ── Result returned to caller ──────────────────────────────────────────────────
    public class PrescriptionDispenseResult
    {
        public string PatientId { get; set; } = string.Empty;
        public List<PrescriptionMedicineRow> DispensedMedicines { get; set; } = new();
    }

    // ── Modal ──────────────────────────────────────────────────────────────────────
    public sealed partial class PatientPrescriptionModal : ContentDialog
    {
        // ── Public output ──────────────────────────────────────────────────────────
        public PrescriptionDispenseResult? Result { get; private set; }

        // ── Dependencies ───────────────────────────────────────────────────────────
        private readonly Prescription _prescription;
        private readonly PrescriptionController _controller;

        // ── Context (display only) ─────────────────────────────────────────────────
        private readonly string _patientName;
        private readonly string _patientGender;
        private readonly string _assignedDoctor;

        // ── Data ───────────────────────────────────────────────────────────────────
        private List<PrescriptionMedicineRow> _medicines = new();

        // ── Constructor ────────────────────────────────────────────────────────────
        public PatientPrescriptionModal(
            Prescription prescription,
            PrescriptionController controller,
            string patientName,
            string patientGender,
            string assignedDoctor)
        {
            InitializeComponent();

            _prescription = prescription;
            _controller = controller;
            _patientName = patientName;
            _patientGender = patientGender;
            _assignedDoctor = assignedDoctor;

            PopulateRecapHeader();
            LoadFromPrescription();
            RefreshList();
        }

        // ── Header ─────────────────────────────────────────────────────────────────
        private void PopulateRecapHeader()
        {
            var parts = _patientName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var initials = parts.Length >= 2
                ? $"{parts[0][0]}{parts[^1][0]}"
                : _patientName.Length > 0 ? _patientName[0].ToString() : "?";

            var avatarColor = _patientGender.ToLower() switch
            {
                "female" => ColorHelper.FromArgb(0xFF, 0xC2, 0x18, 0x5B),
                "male" => ColorHelper.FromArgb(0xFF, 0x00, 0x78, 0xD4),
                _ => ColorHelper.FromArgb(0xFF, 0x5B, 0x2D, 0x8E)
            };

            RecapAvatar.Background = new SolidColorBrush(avatarColor);
            RecapInitials.Text = initials.ToUpper();
            RecapPatientName.Text = _patientName;
            RecapDoctor.Text = _assignedDoctor;

            // ProcedureName and Date are not passed in from the list page —
            // hide them gracefully so the recap header still looks clean
            RecapProcedureName.Text = string.Empty;
            RecapDate.Text = string.Empty;
        }

        // ── Load medicines from the already-fetched Prescription object ────────────
        private void LoadFromPrescription()
        {
            _medicines = _prescription.PatientMedicines
                .Select(pm => new PrescriptionMedicineRow
                {
                    MedicineId = pm.MedicineId,
                    Name = pm.Medicine?.Name ?? $"Medicine #{pm.MedicineId}",
                    PrescribedQty = pm.Quantity,
                    StockAvailable = pm.Medicine?.Stock ?? 0,
                })
                .ToList();
        }

        // ── List refresh ───────────────────────────────────────────────────────────
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
            TxtDispensedCount.Text = $"{dispensed} / {_medicines.Count} dispensed";
        }

        private void UpdateMarkCompleteButton()
        {
            IsPrimaryButtonEnabled = _medicines.Count > 0 && _medicines.All(m => m.Dispensed);
        }

        private void UpdateDispenseAllButton()
        {
            if (BtnDispenseAll is null) return;
            BtnDispenseAll.IsEnabled = _medicines.Any(m => !m.Dispensed && m.CanDispense);
        }

        // ── Per-row dispense ───────────────────────────────────────────────────────
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

        // ── Dispense All ───────────────────────────────────────────────────────────
        private void OnDispenseAllClicked(object sender, RoutedEventArgs e)
        {
            var skipped = _medicines.Where(m => !m.Dispensed && !m.CanDispense).ToList();
            var pending = _medicines.Where(m => !m.Dispensed && m.CanDispense).ToList();

            if (pending.Count == 0)
            {
                ShowWarning("Remaining medicines cannot be dispensed due to insufficient stock.");
                return;
            }

            foreach (var row in pending)
                row.Dispensed = true;

            ValidationBar.IsOpen = false;
            RefreshList();

            if (skipped.Count > 0)
                ShowWarning($"{skipped.Count} medicine(s) could not be dispensed due to insufficient stock.");
        }

        // ── Mark Complete ──────────────────────────────────────────────────────────
        private async void OnMarkCompleteClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Defer so we can run async work before the dialog closes
            var deferral = args.GetDeferral();

            try
            {
                IsPrimaryButtonEnabled = false;
                ShowSavingOverlay(true);

                await _controller.MarkCompletedAsync(_prescription.PatientProcedureId);

                Result = new PrescriptionDispenseResult
                {
                    PatientId = _prescription.PatientProcedure!.Patient.Id.ToString(),
                    DispensedMedicines = _medicines.Where(m => m.Dispensed).ToList(),
                };

                // Dialog closes normally after deferral.Complete()
            }
            catch (Exception ex)
            {
                args.Cancel = true;
                IsPrimaryButtonEnabled = true;
                ShowWarning($"Failed to complete prescription: {ex.Message}");
            }
            finally
            {
                ShowSavingOverlay(false);
                deferral.Complete();
            }
        }

        // ── Helpers ────────────────────────────────────────────────────────────────
        private void ShowWarning(string message)
        {
            ValidationBar.Severity = InfoBarSeverity.Warning;
            ValidationBar.Message = message;
            ValidationBar.IsOpen = true;
        }

        private void ShowSavingOverlay(bool visible)
        {
            if (SavingOverlay is not null)
                SavingOverlay.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}