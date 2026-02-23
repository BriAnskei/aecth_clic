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

        private bool _isSaving = false;

        public AddNewClient()
        {
            InitializeComponent();
        }

        // ─────────────────────────────────────────
        // TIER CARD SELECTION
        // ─────────────────────────────────────────
        private void TierCard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            _selectedTier = btn.Tag?.ToString() ?? string.Empty;

            SetCardSelected(CardBasic, _selectedTier == "Basic");
            SetCardSelected(CardStandard, _selectedTier == "Standard");
            SetCardSelected(CardPremium, _selectedTier == "Premium");
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
        // SHOW / HIDE TOGGLES  (header buttons)
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
            FieldConfirmPassword.PasswordRevealMode = mode;   // keep confirm in sync
            TogglePasswordIcon.Glyph = _passwordVisible ? "\uED1A" : "\uE7B3";
        }

        // ─────────────────────────────────────────
        // COPY CREDENTIALS  (username + password)
        // ─────────────────────────────────────────
        private void CopyCredentials_Click(object sender, RoutedEventArgs e)
        {
            string username = FieldUsername.Password;
            string password = FieldPassword.Password;

            if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password)) return;

            string text = $"Username: {username} | Password: {password}";
            var package = new DataPackage();
            package.SetText(text);
            Clipboard.SetContent(package);
        }

        // ─────────────────────────────────────────
        // GENERATE CREDENTIALS
        //   Username: <first_word_of_clinic>_clinic + 4 random digits
        //             e.g. "Santos Aesthetic Clinic" → santos_clinic4821
        //   Password: secure random 12-char string
        // ─────────────────────────────────────────
        private void GenerateCredentials_Click(object sender, RoutedEventArgs e)
        {
            var rng = new Random(Environment.TickCount);

            // ── Username ──────────────────────────
            string clinicName = FieldClinicName.Text.Trim();
            string firstWord = string.Empty;

            if (!string.IsNullOrEmpty(clinicName))
            {
                // Take first word, strip non-alphanumeric, lowercase
                firstWord = clinicName.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
                firstWord = Regex.Replace(firstWord, @"[^a-zA-Z0-9]", "").ToLower();
            }

            if (string.IsNullOrEmpty(firstWord))
                firstWord = "clinic";   // fallback if clinic name not yet filled

            int digits = rng.Next(1000, 9999);
            string username = $"{firstWord}_clinic{digits}";

            // ── Password ──────────────────────────
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%";
            var pw = new char[12];
            for (int i = 0; i < pw.Length; i++)
                pw[i] = chars[rng.Next(chars.Length)];
            string password = new string(pw);

            // ── Fill fields ───────────────────────
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
            string clinicName = FieldClinicName.Text.Trim();
            string username = FieldUsername.Password.Trim();
            string password = FieldPassword.Password;
            string confirmPw = FieldConfirmPassword.Password;
            string tier = _selectedTier;

            // Required field check
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

            // Password match check
            if (password != confirmPw)
            {
                ValidationBar.Message = "Passwords do not match.";
                ValidationBar.IsOpen = true;
                args.Cancel = true;
                return;
            }

            Result = new NewClientResult
            {
                FullName = fullName,
                Email = email,
                PhoneNumber = phone,
                ClinicName = clinicName,
                Username = username,
                Password = password,
                ModuleTier = tier,
                CreatedAt = DateTime.Now
            };

            Debug.WriteLine("══════════════════════════════════");
            Debug.WriteLine("  NEW CLIENT SUBMITTED");
            Debug.WriteLine("══════════════════════════════════");
            Debug.WriteLine($"  Full Name   : {Result.FullName}");
            Debug.WriteLine($"  Email       : {Result.Email}");
            Debug.WriteLine($"  Phone       : {Result.PhoneNumber}");
            Debug.WriteLine($"  Clinic      : {Result.ClinicName}");
            Debug.WriteLine($"  Username    : {Result.Username}");
            Debug.WriteLine($"  Password    : {Result.Password}");
            Debug.WriteLine($"  Module Tier : {Result.ModuleTier}");
            Debug.WriteLine($"  Created At  : {Result.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            Debug.WriteLine("══════════════════════════════════");
        }
    }

            // ── HELPER ─────────────────────────────────────────────────────────
    private void SetSavingState(bool isSaving)
        {
            _isSaving = isSaving;
            IsPrimaryButtonEnabled = !isSaving;
            IsSecondaryButtonEnabled = !isSaving;

            // Show/hide a loading indicator — add x:Name="SavingOverlay" to your XAML (see below)
            SavingOverlay.Visibility = isSaving ? Visibility.Visible : Visibility.Collapsed;
        }

    // ── Result DTO ─────────────────────────────────────────────────────────
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