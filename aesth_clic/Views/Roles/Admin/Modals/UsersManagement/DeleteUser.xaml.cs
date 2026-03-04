using aesth_clic.Tenant.Controller;
using aesth_clic.Views.Roles.Admin.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace aesth_clic.Views.Roles.Admin.Modals
{
    // ─────────────────────────────────────────────────────────
    // DELETE USER CONFIRMATION DIALOG
    // Shows the staff member's name and role, and requires the
    // admin to tick a checkbox before the "Delete Permanently"
    // button becomes enabled. Only deactivated users can be
    // deleted — enforced by the caller in UserManagement.
    // ─────────────────────────────────────────────────────────
    public sealed partial class DeleteUser : ContentDialog
    {
        private readonly StaffUserItem _user;
        private readonly UserController _userController;

        /// <summary>True once the admin ticked the checkbox and pressed Delete.</summary>
        public bool Confirmed { get; private set; }

        /// <summary>Populated if the controller call throws.</summary>
        public Exception? SaveError { get; private set; }

        public DeleteUser(StaffUserItem user, UserController userController)
        {
            _user = user ?? throw new ArgumentNullException(nameof(user));
            _userController = userController ?? throw new ArgumentNullException(nameof(userController));

            InitializeComponent();

            UserNameText.Text = _user.FullName;
            UserRoleText.Text = _user.Role;

            // Primary button stays disabled until the checkbox is ticked
            IsPrimaryButtonEnabled = false;

            PrimaryButtonClick += OnConfirmClicked;
        }

        // ── Checkbox gate ──────────────────────────────────────
        private void ConfirmCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            IsPrimaryButtonEnabled = ConfirmCheckBox.IsChecked == true;
        }

        // ── Confirm handler ────────────────────────────────────
        private async void OnConfirmClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Extra guard: should not be reachable without the checkbox, but just in case
            if (ConfirmCheckBox.IsChecked != true)
            {
                args.Cancel = true;
                return;
            }

            // Mark confirmed — page code-behind uses this to distinguish cancel vs attempted delete
            Confirmed = true;

            // Keep dialog open while the call is in-flight
            args.Cancel = true;

            IsPrimaryButtonEnabled = false;
            IsSecondaryButtonEnabled = false;
            SavingOverlay.Visibility = Visibility.Visible;

            try
            {
                await _userController.DeleteUserAsync(int.Parse(_user.UserId));
            }
            catch (Exception ex)
            {
                SaveError = ex;
            }
            finally
            {
                // Always close — page code-behind reads SaveError for toast
                Hide();
            }
        }
    }
}