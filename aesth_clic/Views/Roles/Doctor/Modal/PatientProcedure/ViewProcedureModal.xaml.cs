using aesth_clic.Session;
using aesth_clic.Tenant.Controller;
using aesth_clic.Tenant.DTO;
using aesth_clic.Tenant.Model;
using aesth_clic.Views.Roles.Doctor.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;

namespace aesth_clic.Views.Roles.Doctor.Modals
{
    // ── View-only medicine row (read-only prescription table) ──────────────────
    public class PrescriptionMedicineViewItem
    {
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string QuantityDisplay => $"x{Quantity}";
    }

    // ── Modal ──────────────────────────────────────────────────────────────────
    public sealed partial class ViewProcedureModal : ContentDialog
    {
        // ── Context ────────────────────────────────────────────────────────────
        private readonly PatientProcedureItem _item;
        private Prescription? _prescription;

        // ── Controllers ────────────────────────────────────────────────────────
        private readonly MedicineController _medicineController;
        private readonly PrescriptionController _prescriptionController;

        // ── Edit state ─────────────────────────────────────────────────────────
        private bool _isEditMode = false;
        private bool _isSaving = false;
        private readonly List<MedicineRowItem> _allEditMedicines = new();
        private List<MedicineRowItem> _displayedEditMedicines = new();

        // ── Constructor ────────────────────────────────────────────────────────
        public ViewProcedureModal(PatientProcedureItem item, Prescription? prescription)
        {
            InitializeComponent();

            _item = item;
            _prescription = prescription;

            _medicineController = App.Services.GetRequiredService<MedicineController>();
            _prescriptionController = App.Services.GetRequiredService<PrescriptionController>();

            PopulateHeader();
            PopulateInfoGrid();
            PopulatePrescription();
        }

        // ── Header ─────────────────────────────────────────────────────────────
        private void PopulateHeader()
        {
            Title = "Procedure Details";

            PatientInitials.Text = _item.Initials;
            PatientAvatar.Background = _item.AvatarColor;
            TxtPatientName.Text = _item.PatientName;
            TxtProcedureName.Text = _item.ProcedureName;

            var (bg, fg) = _item.Status switch
            {
                "Completed" => (Color.FromArgb(255, 232, 245, 233), Color.FromArgb(255, 46, 125, 50)),
                "Scheduled" => (Color.FromArgb(255, 227, 242, 253), Color.FromArgb(255, 0, 120, 212)),
                "Cancelled" => (Color.FromArgb(255, 253, 236, 234), Color.FromArgb(255, 197, 15, 31)),
                _ => (Color.FromArgb(255, 255, 248, 225), Color.FromArgb(255, 245, 158, 11)),
            };
            StatusBadge.Background = new SolidColorBrush(bg);
            TxtStatus.Text = _item.Status;
            TxtStatus.Foreground = new SolidColorBrush(fg);
        }

        // ── Info Grid ──────────────────────────────────────────────────────────
        private void PopulateInfoGrid()
        {
            TxtAssignedDoctor.Text = string.IsNullOrEmpty(_item.AssignedDoctorName)
                ? "Not assigned" : _item.AssignedDoctorName;

            TxtAppointmentDate.Text = string.IsNullOrEmpty(_item.AppointmentSchedule)
                ? "Not scheduled" : _item.AppointmentSchedule;

            TxtProcedureDate.Text = string.IsNullOrEmpty(_item.ProcedureSchedule)
                ? "Not scheduled" : _item.ProcedureSchedule;

            TxtCost.Text = string.IsNullOrEmpty(_item.Cost) ? "—" : _item.Cost;
            TxtCreatedAt.Text = _item.CreatedAtDisplay;
        }

        // ── Prescription ───────────────────────────────────────────────────────
        private void PopulatePrescription()
        {
            if (_item.Status != "Completed" || _prescription is null)
            {
                PrescriptionSection.Visibility = Visibility.Collapsed;
                return;
            }

            PrescriptionSection.Visibility = Visibility.Visible;

            // Normalize status to title-case: "Pending" or "Completed"
            var normalizedStatus = _prescription.Status?.ToLower() switch
            {
                "pending" => "Pending",
                "completed" => "Completed",
                _ => _prescription.Status ?? "Pending"
            };

            // Badge color — only two statuses
            var (pBg, pFg) = normalizedStatus == "Completed"
                ? (Color.FromArgb(255, 232, 245, 233), Color.FromArgb(255, 46, 125, 50))
                : (Color.FromArgb(255, 255, 248, 225), Color.FromArgb(255, 245, 158, 11));

            PrescriptionStatusBadge.Background = new SolidColorBrush(pBg);
            TxtPrescriptionStatus.Text = normalizedStatus;
            TxtPrescriptionStatus.Foreground = new SolidColorBrush(pFg);

            // Rebuild the view-mode table
            RefreshViewModeTable();

            // Show Edit button only if current user is the assigned doctor
            // AND prescription is still Pending
            var currentUserId = AppSession.Instance.CurrentUser?.Id ?? -1;
            var isAssignedDoctor = _item.AssignedDoctorId == currentUserId;
            var isPending = normalizedStatus == "Pending";

            BtnEditPrescription.Visibility =
                (isAssignedDoctor && isPending)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        // ── Rebuild view-mode table from _prescription ─────────────────────────
        private void RefreshViewModeTable()
        {
            if (_prescription is null) return;

            var viewItems = _prescription.PatientMedicines.Select(pm => new PrescriptionMedicineViewItem
            {
                Name = pm.Medicine?.Name ?? "Unknown",
                Unit = pm.Medicine?.Unit ?? string.Empty,
                Quantity = pm.Quantity,
            }).ToList();

            PrescriptionViewList.ItemsSource = viewItems;
        }

        // ── Enter Edit Mode ────────────────────────────────────────────────────
        private async void OnEditPrescriptionClicked(object sender, RoutedEventArgs e)
        {
            if (_isEditMode) return;

            try
            {
                var medicines = await _medicineController.GetAllMedicinesAsync();

                _allEditMedicines.Clear();
                foreach (var m in medicines)
                {
                    _allEditMedicines.Add(new MedicineRowItem
                    {
                        MedicineId = m.Id,
                        MedicineIdTag = m.Id.ToString(),
                        Name = m.Name,
                        // Start fresh — no pre-selection
                    });
                }

                RefreshEditMedicineList(string.Empty);
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load medicines: {ex.Message}");
                return;
            }

            _isEditMode = true;

            ViewModePanel.Visibility = Visibility.Collapsed;
            EditModePanel.Visibility = Visibility.Visible;
            EditActionsRow.Visibility = Visibility.Visible;
            BtnEditPrescription.Visibility = Visibility.Collapsed;
            PrescriptionSuccessBar.IsOpen = false;

            PrimaryButtonText = "Save Changes";
            IsPrimaryButtonEnabled = true;
        }

        // ── Exit Edit Mode ─────────────────────────────────────────────────────
        private void ExitEditMode()
        {
            _isEditMode = false;

            EditModePanel.Visibility = Visibility.Collapsed;
            EditActionsRow.Visibility = Visibility.Collapsed;
            ViewModePanel.Visibility = Visibility.Visible;

            // Edit button stays hidden after save (per spec)
            BtnEditPrescription.Visibility = Visibility.Collapsed;

            // Hide Save Changes primary button
            PrimaryButtonText = string.Empty;
            IsPrimaryButtonEnabled = false;

            PrescriptionValidationBar.IsOpen = false;
            EditMedicineSearchBox.Text = string.Empty;
        }

        private void OnCancelEditClicked(object sender, RoutedEventArgs e)
        {
            if (_isSaving) return;
            ExitEditMode();

            // Re-show Edit button on cancel (user didn't save, still Pending)
            var currentUserId = AppSession.Instance.CurrentUser?.Id ?? -1;
            var isAssignedDoctor = _item.AssignedDoctorId == currentUserId;
            if (isAssignedDoctor)
                BtnEditPrescription.Visibility = Visibility.Visible;
        }

        // ── Save Changes ───────────────────────────────────────────────────────
        private async void OnSaveClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true; // always prevent auto-close

            if (!_isEditMode || _isSaving) return;

            var selected = _allEditMedicines.Where(m => m.Selected).ToList();
            if (selected.Count == 0)
            {
                ShowError("Please select at least one medicine before saving.");
                return;
            }

            _isSaving = true;
            IsPrimaryButtonEnabled = false;
            IsSecondaryButtonEnabled = false;
            PrescriptionValidationBar.IsOpen = false;

            try
            {
                var medicineDtos = selected.Select(m => new PrescriptionMedicineDto
                {
                    MedicineId = m.MedicineId,
                    Quantity = m.Quantity,
                }).ToList();

                // Call the controller — UpdateAsync replaces all PatientMedicines
                var updatedPrescription = await _prescriptionController
                    .UpdateAsync(int.Parse(_item.ProcedureRecordId), medicineDtos);

                // Update local _prescription so RefreshViewModeTable shows new data
                _prescription = updatedPrescription;

                // Rebuild view-mode table with the freshly returned prescription
                RefreshViewModeTable();

                // Exit edit mode (Edit button stays hidden per spec)
                ExitEditMode();

                // Show success InfoBar
                PrescriptionSuccessBar.IsOpen = true;
            }
            catch (Exception ex)
            {
                ShowError($"Failed to update prescription: {ex.Message}");
                IsPrimaryButtonEnabled = true;
                IsSecondaryButtonEnabled = true;
            }
            finally
            {
                _isSaving = false;
            }
        }

        // ── Close / Back ───────────────────────────────────────────────────────
        private void OnCloseClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (_isSaving)
            {
                args.Cancel = true;
                return;
            }

            if (_isEditMode)
            {
                args.Cancel = true;
                OnCancelEditClicked(sender, null!);
                CloseButtonText = "Close";
            }
        }

        // ── Edit list helpers ──────────────────────────────────────────────────
        private void RefreshEditMedicineList(string search)
        {
            _displayedEditMedicines = string.IsNullOrWhiteSpace(search)
                ? _allEditMedicines
                : _allEditMedicines
                    .Where(m => m.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            EditMedicineList.ItemsSource = null;
            EditMedicineList.ItemsSource = _displayedEditMedicines;

            UpdateEditSelectedCount();
        }

        private void UpdateEditSelectedCount()
        {
            var count = _allEditMedicines.Count(m => m.Selected);
            TxtEditSelectedCount.Text = count == 0 ? "0 selected" : $"{count} selected";
        }

        private void EditMedicineSearch_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
            => RefreshEditMedicineList(sender.Text);

        private void EditMedicineRow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            if (!int.TryParse(btn.Tag?.ToString(), out int id)) return;

            var med = _allEditMedicines.FirstOrDefault(m => m.MedicineId == id);
            if (med is null) return;

            med.Selected = !med.Selected;
            RefreshEditMedicineList(EditMedicineSearchBox.Text);
            PrescriptionValidationBar.IsOpen = false;
        }

        private void EditIncrementQty_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            if (!int.TryParse(btn.Tag?.ToString(), out int id)) return;

            var med = _allEditMedicines.FirstOrDefault(m => m.MedicineId == id);
            if (med is null) return;

            med.Quantity++;
            RefreshEditMedicineList(EditMedicineSearchBox.Text);
        }

        private void EditDecrementQty_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            if (!int.TryParse(btn.Tag?.ToString(), out int id)) return;

            var med = _allEditMedicines.FirstOrDefault(m => m.MedicineId == id);
            if (med is null) return;

            med.Quantity--;
            RefreshEditMedicineList(EditMedicineSearchBox.Text);
        }

        // ── Error helper ───────────────────────────────────────────────────────
        private void ShowError(string message)
        {
            PrescriptionValidationBar.Message = message;
            PrescriptionValidationBar.IsOpen = true;
        }
    }
}