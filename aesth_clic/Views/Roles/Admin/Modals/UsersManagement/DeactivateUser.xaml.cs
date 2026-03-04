using aesth_clic.Tenant.Controller;
using aesth_clic.Views.Roles.Admin.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace aesth_clic.Views.Roles.Admin.Modals
{
    public sealed partial class DeactivateUser : ContentDialog
    {
        private readonly StaffUserItem _user;
        private readonly UserController _userController;

        /// <summary>True once the admin pressed Yes, Deactivate.</summary>
        public bool Confirmed { get; private set; }

        /// <summary>Populated if the controller call throws.</summary>
        public Exception? SaveError { get; private set; }

        public DeactivateUser(StaffUserItem user, UserController userController)
        {
            _user = user ?? throw new ArgumentNullException(nameof(user));
            _userController = userController ?? throw new ArgumentNullException(nameof(userController));
            InitializeComponent();
            UserNameText.Text = _user.FullName;
            PrimaryButtonClick += OnConfirmClicked;
        }

        private async void OnConfirmClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Keep dialog open while call is in-flight
            args.Cancel = true;

            Confirmed = true;
            IsPrimaryButtonEnabled = false;
            IsSecondaryButtonEnabled = false;
            SavingOverlay.Visibility = Visibility.Visible;

            try
            {
                await _userController.UpdateAccountStatusAsync(
                    int.Parse(_user.UserId), "deactivated");
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