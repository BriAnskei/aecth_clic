using aesth_clic.Master.Controller;
using aesth_clic.Master.Dto.Company;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI;

namespace aesth_clic.Views.Roles.SuperAdmin.Modals
{
    public sealed partial class AddNewClient : ContentDialog
    {
        public NewClientResult? Result { get; private set; }

        private string _selectedTier = string.Empty;
        private bool _usernameVisible = false;
        private bool _passwordVisible = false;

        private readonly CompanyController _companyController;

        public AddNewClient()
        {
            InitializeComponent();

            _companyController = App.Services
                .GetRequiredService<CompanyController>();
        }

        // ─────────────────────────────────────────
        // TIER CARD SELECTION
        // ─────────────────────────────────────────
        private void TierCard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            _selectedTier = btn.Tag?.ToString() ?? string.Empty;

            if (_selectedTier != null)
                _selectedTier = _selectedTier.ToLower();

            SetCardSelected(CardBasic, _selectedTier == "basic");
            SetCardSelected(CardStandard, _selectedTier == "standard");
            SetCardSelected(CardPremium, _selectedTier == "premium");
        }

        private void SetCardSelected(Button card, bool isSelected)
        {
            if (isSelected)
            {
                card.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 99, 177));
                card.BorderThickness = new Thickness(2);
                card.Background = new SolidColorBrush(Color.FromArgb(25, 0, 120, 212));
            }
            else
            {
                card.BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"];
                card.BorderThickness = new Thickness(1.5);
                card.Background = (Brush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"];
            }
        }

        // ─────────────────────────────────────────
        // SHOW / HIDE TOGGLES
        // ─────────────────────────────────────────
        private void ToggleUsername_Click(object sender, RoutedEventArgs e)
        {
            _usernameVisible = !_usernameVisible;
            FieldUsername.PasswordRevealMode = _usernameVisible
                ? PasswordRevealMode.Visible : PasswordRevealMode.Hidden;
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

            if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password)) return;

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

            string clinicName = FieldClinicName.Text.Trim();
            string firstWord = string.Empty;

            if (!string.IsNullOrEmpty(clinicName))
            {
                firstWord = clinicName.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
                firstWord = Regex.Replace(firstWord, @"[^a-zA-Z0-9]", "").ToLower();
            }

            if (string.IsNullOrEmpty(firstWord))
                firstWord = "clinic";

            string username = $"{firstWord}_clinic{rng.Next(1000, 9999)}";

            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%";
            var pw = new char[12];
            for (int i = 0; i < pw.Length; i++)
                pw[i] = chars[rng.Next(chars.Length)];
            string password = new string(pw);

            FieldUsername.Password = username;
            FieldPassword.Password = password;
            FieldConfirmPassword.Password = password;
        }

        // ─────────────────────────────────────────
        // SAVE  — async, defers dialog close
        // ─────────────────────────────────────────
        private async void OnSaveClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ValidationBar.IsOpen = false;

            // ── 1. Read fields ────────────────────────────────────────────
            string fullName = FieldFullName.Text.Trim();
            string email = FieldEmail.Text.Trim();
            string phone = FieldPhone.Text.Trim();
            string clinicName = FieldClinicName.Text.Trim();
            string username = FieldUsername.Password.Trim();
            string password = FieldPassword.Password;
            string confirmPw = FieldConfirmPassword.Password;
            string tier = _selectedTier;

            // ── 2. Client-side validation ─────────────────────────────────
            if (string.IsNullOrEmpty(fullName) ||
                string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(clinicName) ||
                string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(tier))
            {
                ValidationBar.Message = "Full Name, Email, Clinic Name, Username, Password, and Module Tier are required.";
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

            // ── 3. Cancel close — we'll close manually on success ─────────
            args.Cancel = true;

            // ── 4. Enter saving state ─────────────────────────────────────
            SetSavingState(true);

            // ── 5. Build DTO ──────────────────────────────────────────────
            var dto = new NewClientUserDto(
                adminUser: new aesth_clic.Tenant.Model.User
                {
                    FullName = fullName,
                    Email = email,
                    PhoneNumber = phone,
                    Username = username,
                    Password = password,
                    Role = "admin",
                    CreatedAt = DateTime.UtcNow,
                },
                client: new aesth_clic.Master.Model.Client
                {
                    ClinicName = clinicName,
                    Tier = tier,
                }
            );

            Debug.WriteLine("══════════════════════════════════");
            Debug.WriteLine("  NEW CLIENT SUBMITTED");
            Debug.WriteLine($"  Full Name   : {fullName}");
            Debug.WriteLine($"  Email       : {email}");
            Debug.WriteLine($"  Phone       : {phone}");
            Debug.WriteLine($"  Clinic      : {clinicName}");
            Debug.WriteLine($"  Username    : {username}");
            Debug.WriteLine($"  Module Tier : {tier}");
            Debug.WriteLine($"  Created At  : {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
            Debug.WriteLine("══════════════════════════════════");

            // ── 6. Call controller ────────────────────────────────────────
            try
            {
                await _companyController.CreateClinicAsync(dto);

                // Success — populate Result and close
                Result = new NewClientResult
                {
                    FullName = fullName,
                    Email = email,
                    PhoneNumber = phone,
                    ClinicName = clinicName,
                    Username = username,
                    Password = password,
                    ModuleTier = tier,
                    CreatedAt = DateTime.UtcNow,
                };

                Hide(); // close dialog programmatically
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException vex)
            {
                ValidationBar.Message = vex.Message;
                ValidationBar.IsOpen = true;
                SetSavingState(false);
            }
            catch (Exception ex)
            {
                ValidationBar.Message = $"Failed to create client: {ex.Message}";
                ValidationBar.IsOpen = true;
                SetSavingState(false);
            }
        }

        // ─────────────────────────────────────────
        // SAVING STATE HELPER
        // ─────────────────────────────────────────
        private void SetSavingState(bool isSaving)
        {
            IsPrimaryButtonEnabled = !isSaving;
            IsSecondaryButtonEnabled = !isSaving;   // Close / Cancel button
            SavingOverlay.Visibility = isSaving
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        // ─────────────────────────────────────────
        // RESULT DTO
        // ─────────────────────────────────────────
        public class NewClientResult
        {
            public string FullName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string PhoneNumber { get; set; } = string.Empty;
            public string ClinicName { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string ModuleTier { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
        }
    }
}