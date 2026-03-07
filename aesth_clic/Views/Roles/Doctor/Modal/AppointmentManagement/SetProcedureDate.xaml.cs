using aesth_clic.Tenant.Controller;
using aesth_clic.Tenant.Dto.PatientProcedure;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;

namespace aesth_clic.Views.Roles.Doctor.Modals
{
    public sealed partial class SetProcedureDate : ContentDialog
    {
        private readonly PatientProcedureController _controller;
        private int _patientProcedureId;
        private bool _dateSelected = false;

        public Exception? SaveError { get; private set; }
        public bool Saved { get; private set; }

        public SetProcedureDate(PatientProcedureController controller)
        {
            InitializeComponent();
            _controller = controller;
            ProcedureDatePicker.DateChanged += (_, _) => _dateSelected = true;
        }

        // ── Populate read-only fields before showing ───────────────────────────────
        public void Load(
            int patientProcedureId,
            string patientName,
            string initials,
            SolidColorBrush avatarColor,
            string procedureName)
        {
            _patientProcedureId = patientProcedureId;
            PatientAvatarBorder.Background = avatarColor;
            PatientInitialsText.Text = initials;
            PatientNameText.Text = patientName;
            ProcedureNameText.Text = procedureName;
        }

        // ── Confirm button ─────────────────────────────────────────────────────────
        private async void OnConfirmClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var deferral = args.GetDeferral();

            try
            {
                if (!_dateSelected)
                {
                    ValidationBar.Title = "Date required";
                    ValidationBar.Message = "Please select a procedure date before confirming.";
                    ValidationBar.IsOpen = true;
                    args.Cancel = true;
                    return;
                }

                ValidationBar.IsOpen = false;
                SavingOverlay.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                IsPrimaryButtonEnabled = false;

                var dto = new AddProcedureDateDto
                {
                    PatientProcedureId = _patientProcedureId,
                    ProcedureDate = ProcedureDatePicker.Date.DateTime,
                };

                await _controller.AddProcedureDateAsync(dto);
                Saved = true;
            }
            catch (Exception ex)
            {
                SaveError = ex;
                SavingOverlay.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                IsPrimaryButtonEnabled = true;

                ValidationBar.Title = "Failed to save";
                ValidationBar.Message = ex.Message;
                ValidationBar.IsOpen = true;
                args.Cancel = true;
            }
            finally
            {
                deferral.Complete();
            }
        }
    }
}