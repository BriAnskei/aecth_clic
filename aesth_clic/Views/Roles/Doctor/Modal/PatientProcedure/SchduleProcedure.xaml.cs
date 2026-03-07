using aesth_clic.Session;
using aesth_clic.Tenant.Controller;
using aesth_clic.Tenant.Dto.PatientProcedure;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI;

namespace aesth_clic.Views.Roles.Doctor.Modals
{
    internal sealed partial class ScheduleProcedure : ContentDialog
    {
        // ─────────────────────────────────────────
        // PUBLIC RESULT / ERROR  (read after ShowAsync)
        // ─────────────────────────────────────────
        public bool Confirmed { get; private set; }
        public Exception? SaveError { get; private set; }

        // ─────────────────────────────────────────
        // INTERNAL STATE
        // ─────────────────────────────────────────
        private readonly PatientProcedureController _procedureController;
        private readonly int _procedureRecordId;
        private readonly int _doctorId;

        // Bound to DatePicker MinYear in XAML via x:Bind
        public DateTimeOffset _minYear { get; } =
            new DateTimeOffset(DateTime.Today.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);

        // ─────────────────────────────────────────
        // CONSTRUCTOR
        // ─────────────────────────────────────────
        internal ScheduleProcedure(
            PatientProcedureController procedureController,
            int procedureRecordId,
            string patientName,
            string patientInitials,
            SolidColorBrush avatarColor,
            string procedureName)
        {
            InitializeComponent();

            _procedureController = procedureController;
            _procedureRecordId = procedureRecordId;

            // Patient info
            PatientNameText.Text = patientName;
            PatientInitialsText.Text = patientInitials;
            PatientAvatarBorder.Background = avatarColor;

            // Procedure info
            ProcedureNameText.Text = procedureName;

            // Default date picker to today
            AppointmentDatePicker.Date = DateTimeOffset.Now;

            // Doctor from session
            var user = AppSession.Instance.CurrentUser;
            if (user is not null)
            {
                _doctorId = user.Id;
                DoctorNameText.Text = user.FullName ?? "Unknown";

                var parts = (user.FullName ?? "?").Split(
                    ' ', StringSplitOptions.RemoveEmptyEntries);
                DoctorInitialsText.Text = parts.Length >= 2
                    ? $"{parts[0][0]}{parts[^1][0]}"
                    : parts.Length > 0 ? parts[0][0].ToString() : "?";
                DoctorInitialsText.Text =
                    DoctorInitialsText.Text.ToUpper();
            }
        }

        // ─────────────────────────────────────────
        // CONFIRM
        // ─────────────────────────────────────────
        private async void OnConfirmClicked(
            ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ValidationBar.IsOpen = false;

            var selectedDate = AppointmentDatePicker.Date.DateTime;

            if (selectedDate.Date < DateTime.Today)
            {
                ValidationBar.Message = "Appointment date cannot be in the past.";
                ValidationBar.IsOpen = true;
                args.Cancel = true;
                return;
            }

            if (_doctorId <= 0)
            {
                ValidationBar.Message = "Could not resolve the logged-in doctor. Please re-login.";
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
                var dto = new SchedulePatientProcedureDto
                {
                    PatientProcedureId = _procedureRecordId,
                    AssignedDoctorId = _doctorId,
                    AppointmentDate = selectedDate,
                };

                await _procedureController.ScheduleProcedureAsync(dto);
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