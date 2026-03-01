using aesth_clic.Master.Controller;
using aesth_clic.Master.Dto.Company;
using aesth_clic.Views.Roles.SuperAdmin.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;

namespace aesth_clic.Views.Roles.SuperAdmin.Modals
{
    // ─────────────────────────────────────────────────────────
    // RESULT MODEL
    // ─────────────────────────────────────────────────────────
    public class EditClientResult
    {
        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public string ClinicName { get; init; } = string.Empty;
        public string Username { get; init; } = string.Empty;

        /// <summary>
        /// Null  → user left password blank, keep existing.
        /// Non-null → new password was applied.
        /// </summary>
        public string? Password { get; init; }
    }

    // ─────────────────────────────────────────────────────────
    // DIALOG CODE-BEHIND
    // ─────────────────────────────────────────────────────────
    public sealed partial class EditClient : ContentDialog
    {
        private readonly UserItem _user;
        private readonly AdminUserController _adminUserController;

        public EditClientResult? Result { get; private set; }

        private bool _usernameRevealed = false;
        private bool _passwordRevealed = false;

        // ── Constructor ────────────────────────────────────────
        public EditClient(UserItem user)
        {
            _user = user ?? throw new ArgumentNullException(nameof(user));

            _adminUserController = App.Services
                .GetRequiredService<AdminUserController>();

            InitializeComponent();
            PrePopulateFields();
        }

        // ── Pre-populate ───────────────────────────────────────
        private void PrePopulateFields()
        {
            FieldFullName.Text = _user.FullName;
            FieldEmail.Text = _user.Email;
            FieldPhone.Text = _user.Phone;
            FieldUsername.Password = _user.Username;
            // ClinicName intentionally omitted — not in UpdateAdminUserDto
        }

        // ── Save handler (async — defers dialog close) ─────────
        private async void OnSaveClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ValidationBar.IsOpen = false;

            // ── 1. Read fields ────────────────────────────────────
            string fullName = FieldFullName.Text.Trim();
            string email = FieldEmail.Text.Trim();
            string phone = FieldPhone.Text.Trim();
            string username = FieldUsername.Password.Trim();
            string password = FieldPassword.Password;
            string confirmPw = FieldConfirmPassword.Password;

            // ── 2. Client-side validation ─────────────────────────
            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(phone) ||
                string.IsNullOrWhiteSpace(username))
            {
                ValidationBar.Title = "Missing information";
                ValidationBar.Message = "Full Name, Email, Phone, and Username are all required.";
                ValidationBar.IsOpen = true;
                args.Cancel = true;
                return;
            }

            string? newPassword = null;
            if (!string.IsNullOrEmpty(password))
            {
                if (password != confirmPw)
                {
                    ValidationBar.Title = "Password mismatch";
                    ValidationBar.Message = "The new password and confirmation do not match.";
                    ValidationBar.IsOpen = true;
                    args.Cancel = true;
                    return;
                }
                newPassword = password;
            }

            // ── 3. Block dialog close — we close manually on success ──
            args.Cancel = true;

            // ── 4. Enter saving state ─────────────────────────────
            SetSavingState(true);

            // ── 5. Build DTO ──────────────────────────────────────
            var dto = new UpdateAdminUserDto
            {
                ClinicCode = _user.ClinicCode,
                FullName = fullName,
                Email = email,
                PhoneNumber = phone,
                Username = username,
                Password = newPassword,   // null = keep existing
            };

            Debug.WriteLine("─────────────────────────────────────");
            Debug.WriteLine("[EditClient] Saving changes:");
            Debug.WriteLine($"  ClinicCode : {dto.ClinicCode}");
            Debug.WriteLine($"  Full Name  : {dto.FullName}");
            Debug.WriteLine($"  Email      : {dto.Email}");
            Debug.WriteLine($"  Phone      : {dto.PhoneNumber}");
            Debug.WriteLine($"  Username   : {dto.Username}");
            Debug.WriteLine($"  Password   : {(dto.Password is null ? "(unchanged)" : "*** (updated)")}");
            Debug.WriteLine("─────────────────────────────────────");

            // ── 6. Call controller ────────────────────────────────
            try
            {
                await _adminUserController.UpdateClientAsync(dto);

                // Success — populate Result and close
                Result = new EditClientResult
                {
                    FullName = fullName,
                    Email = email,
                    PhoneNumber = phone,
                    ClinicName = _user.ClinicName,   // unchanged, carry through
                    Username = username,
                    Password = newPassword,
                };

                Hide();
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException vex)
            {
                ValidationBar.Title = "Validation error";
                ValidationBar.Message = vex.Message;
                ValidationBar.IsOpen = true;
                SetSavingState(false);
            }
            catch (Exception ex)
            {
                ValidationBar.Title = "Failed to save changes";
                ValidationBar.Message = ex.Message;
                ValidationBar.IsOpen = true;
                SetSavingState(false);
            }
        }

        // ── Saving state helper ────────────────────────────────
        private void SetSavingState(bool isSaving)
        {
            IsPrimaryButtonEnabled = !isSaving;
            IsSecondaryButtonEnabled = !isSaving;
            SavingOverlay.Visibility = isSaving
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        // ── Toggle Username visibility ─────────────────────────
        private void ToggleUsername_Click(object sender, RoutedEventArgs e)
        {
            _usernameRevealed = !_usernameRevealed;
            FieldUsername.PasswordRevealMode = _usernameRevealed
                ? PasswordRevealMode.Visible
                : PasswordRevealMode.Hidden;
            ToggleUsernameIcon.Glyph = _usernameRevealed ? "\uED1A" : "\uE7B3";
        }

        // ── Toggle Password visibility ─────────────────────────
        private void TogglePassword_Click(object sender, RoutedEventArgs e)
        {
            _passwordRevealed = !_passwordRevealed;
            var mode = _passwordRevealed
                ? PasswordRevealMode.Visible
                : PasswordRevealMode.Hidden;
            FieldPassword.PasswordRevealMode = mode;
            FieldConfirmPassword.PasswordRevealMode = mode;
            TogglePasswordIcon.Glyph = _passwordRevealed ? "\uED1A" : "\uE7B3";
        }

        // ── Generate credentials ───────────────────────────────
        private void GenerateCredentials_Click(object sender, RoutedEventArgs e)
        {
            const string letters = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string special = "!@#$%";
            const string all = letters + digits + special;

            var rng = new Random();

            string username = "user" + string.Create(4, rng, (buf, r) =>
            {
                for (int i = 0; i < buf.Length; i++)
                    buf[i] = digits[r.Next(digits.Length)];
            });

            char[] pwd = new char[10];
            pwd[0] = letters[rng.Next(letters.Length)];
            pwd[1] = digits[rng.Next(digits.Length)];
            pwd[2] = special[rng.Next(special.Length)];
            for (int i = 3; i < pwd.Length; i++)
                pwd[i] = all[rng.Next(all.Length)];

            for (int i = pwd.Length - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (pwd[i], pwd[j]) = (pwd[j], pwd[i]);
            }
            string password = new(pwd);

            FieldUsername.Password = username;
            FieldPassword.Password = password;
            FieldConfirmPassword.Password = password;

            FieldUsername.PasswordRevealMode = PasswordRevealMode.Visible;
            FieldPassword.PasswordRevealMode = PasswordRevealMode.Visible;
            FieldConfirmPassword.PasswordRevealMode = PasswordRevealMode.Visible;

            _usernameRevealed = true;
            _passwordRevealed = true;
            ToggleUsernameIcon.Glyph = "\uED1A";
            TogglePasswordIcon.Glyph = "\uED1A";
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