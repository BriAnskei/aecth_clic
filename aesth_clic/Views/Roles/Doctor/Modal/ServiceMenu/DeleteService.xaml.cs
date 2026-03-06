using aesth_clic.Tenant.Controller;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace aesth_clic.Views.Roles.Doctor.Modals
{
    internal sealed partial class DeleteService : ContentDialog
    {
        // ─────────────────────────────────────────
        // PUBLIC RESULT / ERROR  (read after ShowAsync)
        // ─────────────────────────────────────────
        public bool Deleted { get; private set; }
        public Exception? SaveError { get; private set; }

        // ─────────────────────────────────────────
        // INTERNAL STATE
        // ─────────────────────────────────────────
        private readonly MenuController _menuController;
        private int _serviceId = 0;
        private string _serviceName = string.Empty;

        // ─────────────────────────────────────────
        // CONSTRUCTOR
        // ─────────────────────────────────────────
        internal DeleteService(MenuController menuController)
        {
            InitializeComponent();
            _menuController = menuController;
        }

        // ─────────────────────────────────────────
        // LOAD  — call before ShowAsync()
        // ─────────────────────────────────────────
        internal void LoadService(int serviceId, string serviceName)
        {
            _serviceId = serviceId;
            _serviceName = serviceName;

            ServiceNameLabel.Text = serviceName;
        }

        // ─────────────────────────────────────────
        // DELETE
        // ─────────────────────────────────────────
        private async void OnDeleteClicked(
            ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ErrorBar.IsOpen = false;

            // Block close + show overlay
            args.Cancel = true;
            DeletingOverlay.Visibility = Visibility.Visible;
            IsPrimaryButtonEnabled = false;
            IsSecondaryButtonEnabled = false;

            try
            {
                Deleted = await _menuController.DeleteServiceAsync(_serviceId);
            }
            catch (Exception ex)
            {
                SaveError = ex;
                Deleted = false;
            }
            finally
            {
                Hide();
            }
        }
    }
}