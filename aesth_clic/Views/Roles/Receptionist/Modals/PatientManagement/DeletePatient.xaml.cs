using aesth_clic.Tenant.Controller;
using aesth_clic.Views.Roles.Receptionist.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace aesth_clic.Views.Roles.Receptionist.Modals
{
    public sealed partial class DeletePatient : ContentDialog
    {
        // ── Public results ─────────────────────────────────────────────────
        public bool Confirmed { get; private set; } = false;
        public Exception? SaveError { get; private set; }

        // ── Private state ──────────────────────────────────────────────────
        private readonly PatientItem _patient;
        private readonly PatientController _patientController;

        // ── Constructor ────────────────────────────────────────────────────
        public DeletePatient(PatientItem patient, PatientController patientController)
        {
            InitializeComponent();

            // Apply button styles here — AFTER InitializeComponent() so that
            // ContentDialog.Resources has already been parsed and the keys exist.
            if (Resources.TryGetValue("RedDestructiveButtonStyle", out var redStyle))
                PrimaryButtonStyle = (Style)redStyle;

            if (Resources.TryGetValue("GrayCloseButtonStyle", out var grayStyle))
                CloseButtonStyle = (Style)grayStyle;

            _patient = patient;
            _patientController = patientController;

            TxtPatientName.Text = $"Delete {patient.FullName}?";
            TxtDetailMessage.Text =
                $"Permanently deleting {patient.FullName} will remove all their records " +
                $"from the system, including associated payment and procedure history.";
        }

        // ── Primary button (Delete) ────────────────────────────────────────
        private async void OnDeleteClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Prevent the dialog from auto-closing so we can show progress first.
            args.Cancel = true;

            SavingOverlay.Visibility = Visibility.Visible;
            IsPrimaryButtonEnabled = false;
            IsSecondaryButtonEnabled = false;

            try
            {
                await _patientController.DeletePatientAsync(int.Parse(_patient.PatientId));
                Confirmed = true;
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