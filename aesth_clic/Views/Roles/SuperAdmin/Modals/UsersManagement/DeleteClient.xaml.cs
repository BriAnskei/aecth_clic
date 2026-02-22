using aesth_clic.Views.Roles.SuperAdmin.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace aesth_clic.Views.Roles.SuperAdmin.Modals
{
    // ─────────────────────────────────────────────────────────
    // DELETE CLIENT CONFIRMATION DIALOG
    // Shows the client's name and requires the super-admin to
    // tick a checkbox before the "Delete Permanently" button
    // becomes enabled. Only deactivated clients can be deleted.
    // ─────────────────────────────────────────────────────────
    public sealed partial class DeleteClient : ContentDialog
    {
        private readonly UserItem _user;

        /// <summary>
        /// True when the admin confirmed and the dialog closed via Primary button.
        /// </summary>
        public bool Confirmed { get; private set; }

        public DeleteClient(UserItem user)
        {
            _user = user ?? throw new ArgumentNullException(nameof(user));
            InitializeComponent();
            ClientNameText.Text = _user.FullName;

            // Start with the Primary button disabled until checkbox is ticked
            IsPrimaryButtonEnabled = false;

            PrimaryButtonClick += OnConfirmClicked;
        }

        // ── Checkbox gate ──────────────────────────────────────
        private void ConfirmCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            IsPrimaryButtonEnabled = ConfirmCheckBox.IsChecked == true;
        }

        // ── Confirm handler ────────────────────────────────────
        private void OnConfirmClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Extra guard: should not be reachable without the checkbox, but just in case
            if (ConfirmCheckBox.IsChecked != true)
            {
                args.Cancel = true;
                return;
            }
            Confirmed = true;
        }
    }
}