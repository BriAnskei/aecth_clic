using aesth_clic.Tenant.Controller;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Text.RegularExpressions;

namespace aesth_clic.Views.Roles.Receptionist.Modals
{
    public sealed partial class AddEditPatient : ContentDialog
    {
        public PatientResult? Result { get; private set; }
        public Exception? SaveError { get; private set; }

        private readonly PatientController _patientController;
        private bool _isEditMode = false;
        private int _editPatientId = 0;

        public AddEditPatient(PatientController patientController)
        {
            InitializeComponent();
            _patientController = patientController;
        }

        // ── Edit mode — call before ShowAsync() ────────────────────────────────────
        public void LoadForEdit(int patientId, string fullName, string email,
                                string phone, int age, string gender, string address)
        {
            _isEditMode = true;
            _editPatientId = patientId;

            Title = "Edit Patient";
            PrimaryButtonText = "Save Changes";

            FieldFullName.Text = fullName;
            FieldEmail.Text = email;
            FieldPhone.Text = phone;
            FieldAge.Text = age > 0 ? age.ToString() : string.Empty;
            FieldAddress.Text = address;

            foreach (var item in FieldGender.Items)
            {
                if (item is ComboBoxItem cbi && cbi.Tag?.ToString() == gender)
                {
                    FieldGender.SelectedItem = cbi;
                    break;
                }
            }
        }

        // ── Save ───────────────────────────────────────────────────────────────────
        private async void OnSaveClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ValidationBar.IsOpen = false;

            string fullName = FieldFullName.Text.Trim();
            string email = FieldEmail.Text.Trim();
            string phone = FieldPhone.Text.Trim();
            string ageText = FieldAge.Text.Trim();
            string gender = (FieldGender.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? string.Empty;
            string address = FieldAddress.Text.Trim();

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(ageText) ||
                string.IsNullOrEmpty(gender) || string.IsNullOrEmpty(address))
            {
                ValidationBar.Message = "All fields are required.";
                ValidationBar.IsOpen = true;
                args.Cancel = true;
                return;
            }

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ValidationBar.Message = "Invalid email format.";
                ValidationBar.IsOpen = true;
                args.Cancel = true;
                return;
            }

            if (!int.TryParse(ageText, out int age) || age <= 0 || age > 120)
            {
                ValidationBar.Message = "Age must be a number between 1 and 120.";
                ValidationBar.IsOpen = true;
                args.Cancel = true;
                return;
            }

            Result = new PatientResult
            {
                PatientId = _editPatientId,
                FullName = fullName,
                Email = email,
                Phone = phone,
                Age = age,
                Gender = gender,
                Address = address,
            };

            args.Cancel = true;
            SavingOverlay.Visibility = Visibility.Visible;
            IsPrimaryButtonEnabled = false;
            IsSecondaryButtonEnabled = false;

            try
            {
                if (_isEditMode)
                    await _patientController.UpdatePatientAsync(
                        _editPatientId, fullName, gender, age, email, address, phone);
                else
                    await _patientController.CreatePatientAsync(
                        fullName, gender, age, email, address, phone);
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

        // ── Result DTO ─────────────────────────────────────────────────────────────
        public class PatientResult
        {
            public int PatientId { get; set; }
            public string FullName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public int Age { get; set; }
            public string Gender { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
        }
    }
}