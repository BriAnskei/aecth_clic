using System;
using System.Text.RegularExpressions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;

namespace aesth_clic.Views.Roles.Admin.Modals
{
    public sealed partial class AddNewUser : ContentDialog
    {
        public NewUserResult? Result { get; private set; }

        private bool _usernameVisible = false;
        private bool _passwordVisible = false;
        private bool _isEditMode = false;

        public AddNewUser()
        {
            InitializeComponent();
        }

        // ─────────────────────────────────────────
        // EDIT MODE — call this before ShowAsync()
        // ─────────────────────────────────────────
        public void LoadForEdit(
            string fullName,
            string email,
            string phone,
            string role,
            string username
        )
        {
            _isEditMode = true;

            Title = "Edit User";
            PrimaryButtonText = "Save Changes";

            FieldFullName.Text = fullName;
            FieldEmail.Text = email;
            FieldPhone.Text = phone;
            FieldUsername.Password = username;

            foreach (var item in FieldRole.Items)
            {
                if (item is ComboBoxItem cbi && cbi.Tag?.ToString() == role)
                {
                    FieldRole.SelectedItem = cbi;
                    break;
                }
            }

            // Password fields are optional in edit mode — show hint
            PasswordEditHint.Visibility = Visibility.Visible;
        }

        // ─────────────────────────────────────────
        // SHOW / HIDE TOGGLES
        // ─────────────────────────────────────────
        private void ToggleUsername_Click(object sender, RoutedEventArgs e)
        {
            _usernameVisible = !_usernameVisible;
            FieldUsername.PasswordRevealMode = _usernameVisible
                ? PasswordRevealMode.Visible
                : PasswordRevealMode.Hidden;
            ToggleUsernameIcon.Glyph = _usernameVisible ? "\uED1A" : "\uE7B3";
        }

        private void TogglePassword_Click(object sender, RoutedEventArgs e)
        {
            _passwordVisible = !_passwordVisible;
            var mode = _passwordVisible ? PasswordRevealMode.Visible : PasswordRevealMode.Hidden;
            FieldPassword.PasswordRevealMode = mode;
            FieldConfirmPassword.PasswordRevealMode = mode;
            TogglePasswordIcon.Glyph = _passwordVisible ? "\uED1A" : "\uE7B3";
        }

        // ─────────────────────────────────────────
        // COPY CREDENTIALS
        // ─────────────────────────────────────────
        private void CopyCredentials_Click(object sender, RoutedEventArgs e)
        {
            string username = FieldUsername.Password;
            string password = FieldPassword.Password;
            if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password))
                return;

            var package = new DataPackage();
            package.SetText($"Username: {username} | Password: {password}");
            Clipboard.SetContent(package);
        }

        // ─────────────────────────────────────────
        // GENERATE CREDENTIALS
        // ─────────────────────────────────────────
        private void GenerateCredentials_Click(object sender, RoutedEventArgs e)
        {
            var rng = new Random(Environment.TickCount);

            string fullName = FieldFullName.Text.Trim();
            string firstWord = string.Empty;

            if (!string.IsNullOrEmpty(fullName))
            {
                firstWord = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
                firstWord = Regex.Replace(firstWord, @"[^a-zA-Z0-9]", "").ToLower();
            }

            if (string.IsNullOrEmpty(firstWord))
                firstWord = "staff";

            int digits = rng.Next(1000, 9999);
            string username = $"{firstWord}_staff{digits}";

            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%";
            var pw = new char[12];
            for (int i = 0; i < pw.Length; i++)
                pw[i] = chars[rng.Next(chars.Length)];
            string password = new(pw);

            FieldUsername.Password = username;
            FieldPassword.Password = password;
            FieldConfirmPassword.Password = password;
        }

        // ─────────────────────────────────────────
        // SAVE
        // ─────────────────────────────────────────
        private void OnSaveClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ValidationBar.IsOpen = false;

            string fullName = FieldFullName.Text.Trim();
            string email = FieldEmail.Text.Trim();
            string phone = FieldPhone.Text.Trim();
            string role = (FieldRole.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? string.Empty;

            // Validate shared fields
            if (
                string.IsNullOrEmpty(fullName)
                || string.IsNullOrEmpty(email)
                || string.IsNullOrEmpty(role)
            )
            {
                ValidationBar.Message = "Full Name, Email, and Role are required.";
                ValidationBar.IsOpen = true;
                args.Cancel = true;
                return;
            }

            string username = FieldUsername.Password.Trim();
            string password = FieldPassword.Password;
            string confirmPw = FieldConfirmPassword.Password;

            if (!_isEditMode)
            {
                // Add mode — username and password are required
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    ValidationBar.Message = "Username and Password are required.";
                    ValidationBar.IsOpen = true;
                    args.Cancel = true;
                    return;
                }

                if (password != confirmPw)
                {
                    ValidationBar.Message = "Passwords do not match.";
                    ValidationBar.IsOpen = true;
                    args.Cancel = true;
                    return;
                }
            }
            else
            {
                // Edit mode — password is optional; only validate if something was typed
                bool changingPassword =
                    !string.IsNullOrEmpty(password) || !string.IsNullOrEmpty(confirmPw);

                if (changingPassword && password != confirmPw)
                {
                    ValidationBar.Message = "Passwords do not match.";
                    ValidationBar.IsOpen = true;
                    args.Cancel = true;
                    return;
                }

                // If left blank, password stays as empty string — caller should
                // check string.IsNullOrEmpty(Result.Password) and skip password update
            }

            Result = new NewUserResult
            {
                FullName = fullName,
                Email = email,
                Phone = phone,
                Role = role,
                Username = username,
                Password = password,
                CreatedAt = DateTime.Now,
            };

            SavingOverlay.Visibility = Visibility.Visible;
            IsPrimaryButtonEnabled = false;
            IsSecondaryButtonEnabled = false;
        }

        // ─────────────────────────────────────────
        // RESULT DTO
        // ─────────────────────────────────────────
        public class NewUserResult
        {
            public string FullName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
        }
    }
}
