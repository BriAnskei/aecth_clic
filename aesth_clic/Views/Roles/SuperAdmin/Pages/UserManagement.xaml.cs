using aesth_clic.Views.Roles.SuperAdmin.Modals;
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
using System.Threading.Tasks;
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
        public string Password { get; set; } = string.Empty;

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
                OnPropertyChanged(nameof(ReactivateVisible)); // ✏️ NEW
                OnPropertyChanged(nameof(DeleteVisible));
            }
        }

        // ── Derived display helpers ──────────────────────────

        public string Initials
        {
            get
            {
                var parts = FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                    return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
                return FullName.Length > 0 ? FullName[0].ToString().ToUpper() : "?";
            }
        }

        public SolidColorBrush AvatarColor =>
            new SolidColorBrush(Color.FromArgb(255, 0, 120, 212));

        // ✏️ Updated: "Inactive" → "Deactivated"
        public SolidColorBrush StatusBadgeColor => Status == "Active"
            ? new SolidColorBrush(Color.FromArgb(20, 14, 164, 122))
            : new SolidColorBrush(Color.FromArgb(20, 216, 59, 1));

        // ✏️ Updated: "Inactive" → "Deactivated"
        public SolidColorBrush StatusBadgeText => Status == "Active"
            ? new SolidColorBrush(Color.FromArgb(255, 10, 130, 96))
            : new SolidColorBrush(Color.FromArgb(255, 180, 40, 0));

        // ✏️ Updated: "Inactive" → "Deactivated"
        public Visibility DeactivateVisible => Status == "Active" ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ReactivateVisible => Status == "Deactivated" ? Visibility.Visible : Visibility.Collapsed; // ✏️ NEW
        public Visibility DeleteVisible => Status == "Deactivated" ? Visibility.Visible : Visibility.Collapsed; // ✏️ Updated

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // ─────────────────────────────────────────────────────────
    // PAGE CODE-BEHIND
    // ─────────────────────────────────────────────────────────
    public sealed partial class UserManagement : Page
    {
        private readonly List<UserItem> _allUsers = new();
        private readonly ObservableCollection<UserItem> _displayedUsers = new();
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
            // ✏️ Updated: "Inactive" → "Deactivated" in seed data
            var seed = new[]
            {
                new { Name = "Maria Santos",    Email = "maria@clinic.com",   Phone = "0917-123-4567", Clinic = "Santos Aesthetic Clinic",   Status = "Active"      },
                new { Name = "Jose Reyes",      Email = "jose@clinic.com",    Phone = "0918-234-5678", Clinic = "Reyes Beauty Hub",           Status = "Active"      },
                new { Name = "Anna Cruz",       Email = "anna@clinic.com",    Phone = "0919-345-6789", Clinic = "Cruz Skin & Wellness",       Status = "Active"      },
                new { Name = "Carlos Bautista", Email = "carlos@clinic.com",  Phone = "0920-456-7890", Clinic = "Bautista Derma Center",      Status = "Deactivated" },
                new { Name = "Lucia Ramos",     Email = "lucia@clinic.com",   Phone = "0921-567-8901", Clinic = "Ramos Glow Clinic",          Status = "Active"      },
                new { Name = "Miguel Torres",   Email = "miguel@clinic.com",  Phone = "0922-678-9012", Clinic = "Torres Med Spa",             Status = "Deactivated" },
                new { Name = "Sofia Garcia",    Email = "sofia@clinic.com",   Phone = "0923-789-0123", Clinic = "Garcia Aesthetic Studio",    Status = "Active"      },
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
            // ✏️ Updated: filter tag is now "Deactivated"
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

            TxtRowCount.Text =
                $"Showing {_displayedUsers.Count} of {_allUsers.Count} client{(_allUsers.Count != 1 ? "s" : "")}";
        }

        private void UpdateKpiCards()
        {
            TxtTotalUsers.Text = _allUsers.Count.ToString();
            TxtActiveUsers.Text = _allUsers.Count(u => u.Status == "Active").ToString();
            // ✏️ Updated: count "Deactivated" instead of "Inactive"
            TxtInactiveUsers.Text = _allUsers.Count(u => u.Status == "Deactivated").ToString();
        }

        // ─────────────────────────────────────────
        // TOOLBAR EVENTS
        // ─────────────────────────────────────────
        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
            => ApplyFilters();

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => ApplyFilters();

        // ─────────────────────────────────────────
        // KEBAB MENU
        // ─────────────────────────────────────────
        private void KebabMenu_Click(object sender, RoutedEventArgs e) { }

        // ─────────────────────────────────────────
        // ADD CLIENT  →  opens AddNewClient dialog
        // ─────────────────────────────────────────
        private async void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddNewClient
            {
                XamlRoot = XamlRoot   // required for ContentDialog in WinUI 3
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary && dialog.Result is not null)
            {
                var r = dialog.Result;

                _allUsers.Add(new UserItem
                {
                    UserId = _nextId++,
                    FullName = r.FullName,
                    Email = r.Username,
                    Phone = r.PhoneNumber,
                    ClinicName = r.ClinicName,
                    Password = r.Password,
                    Status = "Active"
                });

                ApplyFilters();
                UpdateKpiCards();
            }
        }

        // ─────────────────────────────────────────
        // EDIT USER
        // ─────────────────────────────────────────
        private async void EditUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
            int userId = (int)item.Tag;

            var user = _allUsers.FirstOrDefault(u => u.UserId == userId);
            if (user == null) return;

            var dialog = new EditClient(user)
            {
                XamlRoot = XamlRoot
            };

            var dialogResult = await dialog.ShowAsync();

            if (dialogResult == ContentDialogResult.Primary && dialog.Result is not null)
            {
                var r = dialog.Result;

                // Mutate the existing UserItem in-place
                user.FullName = r.FullName;
                user.Email = r.Email;
                user.Phone = r.PhoneNumber;
                user.ClinicName = r.ClinicName;

                // Only update the password if the user typed a new one
                if (r.Password is not null)
                    user.Password = r.Password;

                // Refresh the table and KPI cards
                ApplyFilters();
                UpdateKpiCards();
            }
        }

        // ─────────────────────────────────────────
        // MANAGE MODULES  (stub)
        // ─────────────────────────────────────────
        private async void ManageModules_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
            int userId = (int)item.Tag;

            var user = _allUsers.FirstOrDefault(u => u.UserId == userId);
            if (user == null) return;

            var dialog = new ManageModules(user)
            {
                XamlRoot = XamlRoot
            };

            var dialogResult = await dialog.ShowAsync();

            //if (dialogResult == ContentDialogResult.Primary && dialog.Result is not null)
            //{
            //    // Mutate the Tier on the existing UserItem
            //    user.Tier = dialog.Result.Tier;

            //    // Refresh the table (no KPI change needed for tier)
            //    ApplyFilters();
            //}
        }

        // ─────────────────────────────────────────
        // DEACTIVATE  (Active → Deactivated)
        // ─────────────────────────────────────────
        private async void DeactivateUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
            int userId = (int)item.Tag;
            var user = _allUsers.FirstOrDefault(u => u.UserId == userId);
            if (user == null) return;

            var dialog = new DeactivateClient(user)
            {
                XamlRoot = XamlRoot
            };

            await dialog.ShowAsync();

            if (dialog.Confirmed)
            {
                user.Status = "Deactivated";
                ApplyFilters();
                UpdateKpiCards();
            }
        }

        // ─────────────────────────────────────────
        // ✏️ NEW: REACTIVATE  (Deactivated → Active)
        // ─────────────────────────────────────────
        private async void ReactivateUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
            int userId = (int)item.Tag;
            var user = _allUsers.FirstOrDefault(u => u.UserId == userId);
            if (user == null) return;

            var dialog = new ReactivateClient(user)
            {
                XamlRoot = XamlRoot
            };

            await dialog.ShowAsync();

            if (dialog.Confirmed)
            {
                user.Status = "Active";
                ApplyFilters();
                UpdateKpiCards();
            }
        }

        // ─────────────────────────────────────────
        // DELETE  (only allowed when Deactivated)
        // ─────────────────────────────────────────
        private async void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
    int userId = (int)item.Tag;
    var user = _allUsers.FirstOrDefault(u => u.UserId == userId);
    if (user == null || user.Status != "Deactivated") return;

    var dialog = new DeleteClient(user)
    {
        XamlRoot = XamlRoot
    };

    await dialog.ShowAsync();

    if (dialog.Confirmed)
    {
        _allUsers.Remove(user);
        ApplyFilters();
        UpdateKpiCards();
    }
        }
    }
}