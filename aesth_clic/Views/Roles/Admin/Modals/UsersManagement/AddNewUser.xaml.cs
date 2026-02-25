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

        public AddNewUser()
        {
            InitializeComponent();
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
        //   Username: <first_word_of_fullname>_staff + 4 random digits
        //   Password: secure random 12-char string
        // ─────────────────────────────────────────
        private void GenerateCredentials_Click(object sender, RoutedEventArgs e)
        {
            var rng = new Random(Environment.TickCount);

            // Username — based on full name
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

            // Password
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
            string username = FieldUsername.Password.Trim();
            string password = FieldPassword.Password;
            string confirmPw = FieldConfirmPassword.Password;

            if (
                string.IsNullOrEmpty(fullName)
                || string.IsNullOrEmpty(email)
                || string.IsNullOrEmpty(role)
                || string.IsNullOrEmpty(username)
                || string.IsNullOrEmpty(password)
            )
            {
                ValidationBar.Message =
                    "Full Name, Email, Role, Username, and Password are required.";
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
