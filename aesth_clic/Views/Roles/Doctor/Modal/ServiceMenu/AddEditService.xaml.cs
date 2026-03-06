using aesth_clic.Session;
using aesth_clic.Tenant.Controller;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

// Alias to avoid collision with the ServiceMenu Page class
using ServiceMenuModel = aesth_clic.Tenant.Model.ServiceMenu;

namespace aesth_clic.Views.Roles.Doctor.Modals
{
    internal sealed partial class AddEditService : ContentDialog
    {
        // ─────────────────────────────────────────
        // PUBLIC RESULT / ERROR  (read after ShowAsync)
        // ─────────────────────────────────────────
        public ServiceMenuModel? Result { get; private set; }
        public Exception? SaveError { get; private set; }

        // ─────────────────────────────────────────
        // INTERNAL STATE
        // ─────────────────────────────────────────
        private readonly MenuController _menuController;
        private bool _isEditMode = false;
        private int _editServiceId = 0;

        // ─────────────────────────────────────────
        // CONSTRUCTOR
        // ─────────────────────────────────────────
        internal AddEditService(MenuController menuController)
        {
            InitializeComponent();
            _menuController = menuController;
            PopulateAddedBy();
        }

        // ─────────────────────────────────────────
        // POPULATE ADDED-BY FROM SESSION
        // ─────────────────────────────────────────
        private void PopulateAddedBy()
        {
            var user = AppSession.Instance.CurrentUser;
            if (user is null) return;

            string fullName = user.FullName ?? "Unknown";
            AddedByName.Text = fullName;

            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string initials = parts.Length >= 2
                ? $"{parts[0][0]}{parts[^1][0]}"
                : parts.Length > 0 ? parts[0][0].ToString() : "?";

            AddedByInitials.Text = initials.ToUpper();
        }

        // ─────────────────────────────────────────
        // EDIT MODE  — call before ShowAsync()
        // ─────────────────────────────────────────
        internal void LoadForEdit(
            int serviceId,
            string procedureName,
            decimal rawPrice)
        {
            _isEditMode = true;
            _editServiceId = serviceId;

            Title = "Edit Service";
            PrimaryButtonText = "Save Changes";
            OverlayMessage.Text = "Saving changes…";

            FieldProcedureName.Text = procedureName;
            FieldPrice.Text = rawPrice.ToString("0");
        }

        // ─────────────────────────────────────────
        // SAVE
        // ─────────────────────────────────────────
        private async void OnSaveClicked(
            ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ValidationBar.IsOpen = false;

            string name = FieldProcedureName.Text.Trim();
            string priceText = FieldPrice.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                ValidationBar.Message = "Procedure Name is required.";
                ValidationBar.IsOpen = true;
                args.Cancel = true;
                return;
            }

            if (string.IsNullOrEmpty(priceText) ||
                !double.TryParse(priceText, out double rawPrice) ||
                rawPrice < 0)
            {
                ValidationBar.Message = "Please enter a valid positive price.";
                ValidationBar.IsOpen = true;
                args.Cancel = true;
                return;
            }

            // Block close + show overlay
            args.Cancel = true;
            SavingOverlay.Visibility = Visibility.Visible;
            IsPrimaryButtonEnabled = false;
            IsSecondaryButtonEnabled = false;

            try
            {
                if (_isEditMode)
                {
                    bool updated = await _menuController.UpdateServiceAsync(
                        _editServiceId, name, rawPrice);

                    if (updated)
                        Result = await _menuController.GetServiceByIdAsync(_editServiceId);
                }
                else
                {
                    Result = await _menuController.CreateServiceAsync(name, rawPrice);
                }
            }
            catch (Exception ex)
            {
                SaveError = ex;
            }
            finally
            {
                Hide();
            }
        }
    }
}