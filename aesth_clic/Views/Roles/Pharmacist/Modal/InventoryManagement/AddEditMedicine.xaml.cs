using aesth_clic.Tenant.Controller;
using aesth_clic.Tenant.Model;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace aesth_clic.Views.Roles.Pharmacist.Modals
{
    public sealed partial class AddEditMedicine : ContentDialog
    {
        // ── Controller ─────────────────────────────────────────────────────────────
        private readonly MedicineController _controller;

        // ── Mode ───────────────────────────────────────────────────────────────────
        private bool _isEditMode = false;
        private int _editId = 0;

        // ── Results ────────────────────────────────────────────────────────────────
        /// <summary>The saved medicine returned by the controller. Null if cancelled.</summary>
        public Medicine? Result { get; private set; }

        /// <summary>Set when the controller throws. Null on success.</summary>
        public Exception? SaveError { get; private set; }

        // ── Constructor ────────────────────────────────────────────────────────────
        public AddEditMedicine(MedicineController controller)
        {
            InitializeComponent();
            _controller = controller;
        }

        // ── Public: load for edit ──────────────────────────────────────────────────
        /// <summary>
        /// Call before ShowAsync() to switch the dialog into Edit mode.
        /// Stock field is hidden in Edit mode — use the Restock modal for that.
        /// </summary>
        public void LoadForEdit(int id, string name, string unit, DateTime expiryDate)
        {
            _isEditMode = true;
            _editId = id;

            Title = "Edit Medicine";
            PrimaryButtonText = "Update Medicine";

            FieldName.Text = name;
            FieldUnit.Text = unit;

            if (expiryDate > DateTime.MinValue)
                FieldExpiryDate.Date = new DateTimeOffset(expiryDate.ToLocalTime());

            // Hide stock field — not editable in Edit mode
            StockPanel.Visibility = Visibility.Collapsed;
        }

        // ── Save handler ───────────────────────────────────────────────────────────
        private async void OnSaveClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Defer close so we can validate first
            var deferral = args.GetDeferral();

            try
            {
                // ── Validate ───────────────────────────────────────────────────────
                var name = FieldName.Text.Trim();
                var unit = FieldUnit.Text.Trim();

                if (string.IsNullOrWhiteSpace(name))
                {
                    ShowValidation("Medicine name is required.");
                    args.Cancel = true;
                    return;
                }

                if (string.IsNullOrWhiteSpace(unit))
                {
                    ShowValidation("Unit is required.");
                    args.Cancel = true;
                    return;
                }

                if (FieldExpiryDate.Date is null)
                {
                    ShowValidation("Expiry date is required.");
                    args.Cancel = true;
                    return;
                }

                var expiryDate = FieldExpiryDate.Date!.Value.UtcDateTime;

                if (expiryDate <= DateTime.UtcNow)
                {
                    ShowValidation("Expiry date must be in the future.");
                    args.Cancel = true;
                    return;
                }

                if (!_isEditMode)
                {
                    var stock = (int)(FieldStock.Value is double v && !double.IsNaN(v) ? v : 0);
                    if (stock < 0)
                    {
                        ShowValidation("Stock cannot be negative.");
                        args.Cancel = true;
                        return;
                    }
                }

                // ── Save ───────────────────────────────────────────────────────────
                SavingOverlay.Visibility = Visibility.Visible;
                ValidationBar.IsOpen = false;

                if (_isEditMode)
                {
                    await _controller.UpdateMedicineAsync(_editId, name,
                        // stock is not changed in edit mode — pass current value via GetById
                        (await _controller.GetMedicineByIdAsync(_editId))?.Stock ?? 0,
                        unit, expiryDate);

                    Result = await _controller.GetMedicineByIdAsync(_editId);
                }
                else
                {
                    var stock = (int)(FieldStock.Value is double sv && !double.IsNaN(sv) ? sv : 0);
                    Result = await _controller.CreateMedicineAsync(name, stock, unit, expiryDate);
                }
            }
            catch (Exception ex)
            {
                SaveError = ex;
                SavingOverlay.Visibility = Visibility.Collapsed;
                args.Cancel = true;
                ShowValidation(ex.Message);
            }
            finally
            {
                deferral.Complete();
            }
        }

        // ── Helpers ────────────────────────────────────────────────────────────────
        private void ShowValidation(string message)
        {
            ValidationBar.Message = message;
            ValidationBar.IsOpen = true;
        }
    }
}