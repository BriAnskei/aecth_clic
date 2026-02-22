using aesth_clic.Views.Roles.SuperAdmin.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;

namespace aesth_clic.Views.Roles.SuperAdmin.Modals
{
    // ─────────────────────────────────────────────────────────
    // RESULT MODEL  –  returned to the caller on Primary click
    // ─────────────────────────────────────────────────────────
    public class EditClientResult
    {
        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public string ClinicName { get; init; } = string.Empty;
        public string Username { get; init; } = string.Empty;

        /// <summary>
        /// Null means "user left password blank → keep existing password unchanged."
        /// Non-null means "user typed a new password → apply this."
        /// </summary>
        public string? Password { get; init; }
    }

    // ─────────────────────────────────────────────────────────
    // DIALOG CODE-BEHIND
    // ─────────────────────────────────────────────────────────
    public sealed partial class EditClient : ContentDialog
    {
        // The UserItem being edited – passed in by the caller
        private readonly UserItem _user;

        // Expose result to the caller after the dialog closes
        public EditClientResult? Result { get; private set; }

        // Track reveal state for the icon toggles
        private bool _usernameRevealed = false;
        private bool _passwordRevealed = false;

        // ── Constructor ────────────────────────────────────────
        public EditClient(UserItem user)
        {
            _user = user ?? throw new ArgumentNullException(nameof(user));
            InitializeComponent();
            PrePopulateFields();
        }

        // ── Pre-populate from UserItem ─────────────────────────
        private void PrePopulateFields()
        {
            FieldFullName.Text = _user.FullName;
            FieldEmail.Text = _user.Email;
            FieldPhone.Text = _user.Phone;
            FieldClinicName.Text = _user.ClinicName;

            // Username: UserItem doesn't store it separately yet,
            // so we use a sensible mock derived from the email local-part.
            // Replace this with real data once the model is wired to the DB.
            string mockUsername = _user.Email.Contains('@')
                ? _user.Email.Split('@')[0]
                : _user.Email;
            FieldUsername.Password = mockUsername;

            // Password fields intentionally left blank ("leave blank to keep" pattern)
        }

        // ── Save handler ───────────────────────────────────────
        private void OnSaveClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Validate personal info (all four fields required)
            if (string.IsNullOrWhiteSpace(FieldFullName.Text) ||
                string.IsNullOrWhiteSpace(FieldEmail.Text) ||
                string.IsNullOrWhiteSpace(FieldPhone.Text) ||
                string.IsNullOrWhiteSpace(FieldClinicName.Text))
            {
                args.Cancel = true;          // keep dialog open
                ValidationBar.Title = "Missing information";
                ValidationBar.Message = "Full Name, Email, Phone, and Clinic Name are all required.";
                ValidationBar.IsOpen = true;
                return;
            }

            // Validate username
            if (string.IsNullOrWhiteSpace(FieldUsername.Password))
            {
                args.Cancel = true;
                ValidationBar.Title = "Missing information";
                ValidationBar.Message = "Username cannot be empty.";
                ValidationBar.IsOpen = true;
                return;
            }

            // Password validation – only if the user typed something
            string? newPassword = null;
            bool passwordEntered = !string.IsNullOrEmpty(FieldPassword.Password);

            if (passwordEntered)
            {
                if (FieldPassword.Password != FieldConfirmPassword.Password)
                {
                    args.Cancel = true;
                    ValidationBar.Title = "Password mismatch";
                    ValidationBar.Message = "The new password and confirmation do not match.";
                    ValidationBar.IsOpen = true;
                    return;
                }
                newPassword = FieldPassword.Password;
            }

            ValidationBar.IsOpen = false;

            // Build result
            Result = new EditClientResult
            {
                FullName = FieldFullName.Text.Trim(),
                Email = FieldEmail.Text.Trim(),
                PhoneNumber = FieldPhone.Text.Trim(),
                ClinicName = FieldClinicName.Text.Trim(),
                Username = FieldUsername.Password.Trim(),
                Password = newPassword
            };

            // ── Console / Debug output ─────────────────────────
            Debug.WriteLine("─────────────────────────────────────");
            Debug.WriteLine("[EditClient] Saved changes:");
            Debug.WriteLine($"  Full Name  : {Result.FullName}");
            Debug.WriteLine($"  Email      : {Result.Email}");
            Debug.WriteLine($"  Phone      : {Result.PhoneNumber}");
            Debug.WriteLine($"  Clinic     : {Result.ClinicName}");
            Debug.WriteLine($"  Username   : {Result.Username}");
            Debug.WriteLine($"  Password   : {(Result.Password is null ? "(unchanged)" : "*** (updated)")}");
            Debug.WriteLine("─────────────────────────────────────");
        }

        // ── Toggle Username visibility ─────────────────────────
        private void ToggleUsername_Click(object sender, RoutedEventArgs e)
        {
            _usernameRevealed = !_usernameRevealed;
            FieldUsername.PasswordRevealMode = _usernameRevealed
                ? PasswordRevealMode.Visible
                : PasswordRevealMode.Hidden;

            // Eye / Eye-hide glyph swap
            ToggleUsernameIcon.Glyph = _usernameRevealed ? "\uED1A" : "\uE7B3";
        }

        // ── Toggle Password visibility ─────────────────────────
        private void TogglePassword_Click(object sender, RoutedEventArgs e)
        {
            _passwordRevealed = !_passwordRevealed;
            PasswordRevealMode mode = _passwordRevealed
                ? PasswordRevealMode.Visible
                : PasswordRevealMode.Hidden;

            FieldPassword.PasswordRevealMode = mode;
            FieldConfirmPassword.PasswordRevealMode = mode;

            TogglePasswordIcon.Glyph = _passwordRevealed ? "\uED1A" : "\uE7B3";
        }

        // ── Copy credentials to clipboard ─────────────────────
        private void CopyCredentials_Click(object sender, RoutedEventArgs e)
        {
            string username = FieldUsername.Password;
            string password = FieldPassword.Password;

            if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password))
                return;

            var package = new DataPackage();
            package.SetText($"Username: {username}\nPassword: {password}");
            Clipboard.SetContent(package);
        }
    }
}