using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.UI;

namespace aesth_clic.Views.Roles.SuperAdmin.Pages
{
    // ─────────────────────────────────────────────────────────
    // USER MODEL
    // ─────────────────────────────────────────────────────────
    public class UserItem : INotifyPropertyChanged
    {
        public int UserId { get; set; }
        public int RowNumber { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string ClinicName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;   // plain text for mock only

        private string _status = "Active";
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusBadgeColor));
                OnPropertyChanged(nameof(StatusBadgeText));
                OnPropertyChanged(nameof(DeactivateVisible));
                OnPropertyChanged(nameof(DeleteVisible));
            }
        }

        // ── Derived display helpers ──────────────────────────

        public string Initials => FullName.Length >= 2
            ? $"{FullName[0]}{FullName.Split(' ').Last()[0]}".ToUpper()
            : FullName.ToUpper();

        public SolidColorBrush AvatarColor =>
            new SolidColorBrush(Color.FromArgb(255, 0, 120, 212));

        // Status badge
        public SolidColorBrush StatusBadgeColor => Status == "Active"
            ? new SolidColorBrush(Color.FromArgb(20, 14, 164, 122))
            : new SolidColorBrush(Color.FromArgb(20, 216, 59, 1));

        public SolidColorBrush StatusBadgeText => Status == "Active"
            ? new SolidColorBrush(Color.FromArgb(255, 10, 130, 96))
            : new SolidColorBrush(Color.FromArgb(255, 180, 40, 0));

        // Button visibility
        public Visibility DeactivateVisible => Status == "Active" ? Visibility.Visible : Visibility.Collapsed;
        public Visibility DeleteVisible => Status == "Inactive" ? Visibility.Visible : Visibility.Collapsed;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // ─────────────────────────────────────────────────────────
    // PAGE CODE-BEHIND
    // ─────────────────────────────────────────────────────────
    public sealed partial class UserManagement : Page
    {
        // Master list (source of truth)
        private readonly List<UserItem> _allUsers = new();

        // Displayed list (filtered)
        private readonly ObservableCollection<UserItem> _displayedUsers = new();

        // Track which user is being edited (null = adding new)
        private UserItem? _editingUser;

        // Auto-increment id for mock data
        private int _nextId = 1;

        public UserManagement()
        {
            InitializeComponent();
            SeedMockData();
            UserListControl.ItemsSource = _displayedUsers;
            ApplyFilters();
            UpdateKpiCards();
        }

        // ─────────────────────────────────────────
        // MOCK DATA
        // ─────────────────────────────────────────
        private void SeedMockData()
        {
            var seed = new[]
            {
                new { Name = "Maria Santos",    Email = "maria@clinic.com",   Phone = "0917-123-4567", Clinic = "Santos Aesthetic Clinic",   Status = "Active"   },
                new { Name = "Jose Reyes",      Email = "jose@clinic.com",    Phone = "0918-234-5678", Clinic = "Reyes Beauty Hub",           Status = "Active"   },
                new { Name = "Anna Cruz",       Email = "anna@clinic.com",    Phone = "0919-345-6789", Clinic = "Cruz Skin & Wellness",       Status = "Active"   },
                new { Name = "Carlos Bautista", Email = "carlos@clinic.com",  Phone = "0920-456-7890", Clinic = "Bautista Derma Center",      Status = "Inactive" },
                new { Name = "Lucia Ramos",     Email = "lucia@clinic.com",   Phone = "0921-567-8901", Clinic = "Ramos Glow Clinic",          Status = "Active"   },
                new { Name = "Miguel Torres",   Email = "miguel@clinic.com",  Phone = "0922-678-9012", Clinic = "Torres Med Spa",             Status = "Inactive" },
                new { Name = "Sofia Garcia",    Email = "sofia@clinic.com",   Phone = "0923-789-0123", Clinic = "Garcia Aesthetic Studio",    Status = "Active"   },
            };

            foreach (var s in seed)
            {
                _allUsers.Add(new UserItem
                {
                    UserId = _nextId++,
                    FullName = s.Name,
                    Email = s.Email,
                    Phone = s.Phone,
                    ClinicName = s.Clinic,
                    Status = s.Status,
                    Password = "password123"
                });
            }
        }

        // ─────────────────────────────────────────
        // FILTER LOGIC
        // ─────────────────────────────────────────
        private void ApplyFilters()
        {
            string search = SearchBox.Text?.ToLower().Trim() ?? string.Empty;
            string statFilter = (StatusFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All";

            var filtered = _allUsers.Where(u =>
            {
                bool matchSearch = string.IsNullOrEmpty(search)
                    || u.FullName.ToLower().Contains(search)
                    || u.Email.ToLower().Contains(search)
                    || u.ClinicName.ToLower().Contains(search);

                bool matchStatus = statFilter == "All" || u.Status == statFilter;

                return matchSearch && matchStatus;
            }).ToList();

            _displayedUsers.Clear();
            int row = 1;
            foreach (var u in filtered)
            {
                u.RowNumber = row++;
                _displayedUsers.Add(u);
            }

            TxtRowCount.Text = $"Showing {_displayedUsers.Count} of {_allUsers.Count} client{(_allUsers.Count != 1 ? "s" : "")}";
        }

        private void UpdateKpiCards()
        {
            TxtTotalUsers.Text = _allUsers.Count.ToString();
            TxtActiveUsers.Text = _allUsers.Count(u => u.Status == "Active").ToString();
            TxtInactiveUsers.Text = _allUsers.Count(u => u.Status == "Inactive").ToString();
        }

        // ─────────────────────────────────────────
        // TOOLBAR EVENTS
        // ─────────────────────────────────────────
        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
            => ApplyFilters();

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => ApplyFilters();

        // ─────────────────────────────────────────
        // KEBAB MENU · Click (opens flyout via XAML binding;
        //              this handler is a no-op but required
        //              to satisfy the Click binding on the button)
        // ─────────────────────────────────────────
        private void KebabMenu_Click(object sender, RoutedEventArgs e)
        {
            // Flyout is opened automatically by WinUI via Button.Flyout.
            // No additional logic needed here.
        }

        // ─────────────────────────────────────────
        // ADD USER
        // ─────────────────────────────────────────
        private async void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            _editingUser = null;

            // Clear form
            UserDialog.Title = "Add New Client";
            FieldName.Text = string.Empty;
            FieldEmail.Text = string.Empty;
            FieldPhone.Text = string.Empty;
            FieldClinicName.Text = string.Empty;
            FieldPassword.Password = string.Empty;
            DialogValidationBar.IsOpen = false;

            await UserDialog.ShowAsync();
        }

        // ─────────────────────────────────────────
        // EDIT USER
        // ─────────────────────────────────────────
        private async void EditUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
            int userId = (int)item.Tag;
            _editingUser = _allUsers.FirstOrDefault(u => u.UserId == userId);
            if (_editingUser == null) return;

            UserDialog.Title = "Edit Client";
            FieldName.Text = _editingUser.FullName;
            FieldEmail.Text = _editingUser.Email;
            FieldPhone.Text = _editingUser.Phone;
            FieldClinicName.Text = _editingUser.ClinicName;
            FieldPassword.Password = _editingUser.Password;
            DialogValidationBar.IsOpen = false;

            await UserDialog.ShowAsync();
        }

        // ─────────────────────────────────────────
        // MANAGE MODULES  (UI stub — logic TBD)
        // ─────────────────────────────────────────
        private void ManageModules_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement Manage Modules dialog/panel
        }

        // ─────────────────────────────────────────
        // DEACTIVATE USER
        // ─────────────────────────────────────────
        private void DeactivateUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
            int userId = (int)item.Tag;
            var user = _allUsers.FirstOrDefault(u => u.UserId == userId);
            if (user == null) return;

            // TODO: Show confirmation dialog before deactivating
            user.Status = "Inactive";

            ApplyFilters();
            UpdateKpiCards();
        }

        // ─────────────────────────────────────────
        // DELETE USER
        // ─────────────────────────────────────────
        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
            int userId = (int)item.Tag;
            var user = _allUsers.FirstOrDefault(u => u.UserId == userId);
            if (user == null || user.Status != "Inactive") return;

            // TODO: Show confirmation dialog before deleting
            _allUsers.Remove(user);

            ApplyFilters();
            UpdateKpiCards();
        }

        // ─────────────────────────────────────────
        // DIALOG · SAVE
        // ─────────────────────────────────────────
        private void UserDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // ── Validation ──
            string name = FieldName.Text.Trim();
            string email = FieldEmail.Text.Trim();
            string phone = FieldPhone.Text.Trim();
            string clinicName = FieldClinicName.Text.Trim();
            string pass = FieldPassword.Password;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
            {
                DialogValidationBar.Message = "Name, email, and password are required.";
                DialogValidationBar.IsOpen = true;
                args.Cancel = true;   // keep dialog open
                return;
            }

            if (_editingUser == null)
            {
                // ── ADD ──
                _allUsers.Add(new UserItem
                {
                    UserId = _nextId++,
                    FullName = name,
                    Email = email,
                    Phone = phone,
                    ClinicName = clinicName,
                    Password = pass,
                    Status = "Active"
                });
            }
            else
            {
                // ── EDIT ──
                _editingUser.FullName = name;
                _editingUser.Email = email;
                _editingUser.Phone = phone;
                _editingUser.ClinicName = clinicName;
                _editingUser.Password = pass;
            }

            ApplyFilters();
            UpdateKpiCards();
        }
    }
}