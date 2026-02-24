using aesth_clic.Views.Roles.SuperAdmin.Pages;
using Microsoft.UI.Xaml.Controls;
using System;

namespace aesth_clic.Views.Roles.SuperAdmin.Modals
{
    // ─────────────────────────────────────────────────────────
    // REACTIVATE CLIENT CONFIRMATION DIALOG
    // Shows the client's name and asks the super-admin to confirm
    // before changing their status from Deactivated → Active.
    // ─────────────────────────────────────────────────────────
    public sealed partial class ReactivateClient : ContentDialog
    {
        private readonly UserItem _user;

        /// <summary>
        /// True when the admin confirmed the action (Primary button pressed).
        /// </summary>
        public bool Confirmed { get; private set; }

        public ReactivateClient(UserItem user)
        {
            _user = user ?? throw new ArgumentNullException(nameof(user));
            InitializeComponent();
            ClientNameText.Text = _user.FullName;

            PrimaryButtonClick += OnConfirmClicked;
        }

        private void OnConfirmClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Confirmed = true;
            SetSavingState(true);
        }

        private void SetSavingState(bool isSaving)
        {
            IsPrimaryButtonEnabled = !isSaving;
            IsSecondaryButtonEnabled = !isSaving;
            SavingOverlay.Visibility = isSaving ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
        }
    }
}