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

        public SolidColorBrush StatusBadgeColor => Status == "Active"
            ? new SolidColorBrush(Color.FromArgb(20, 14, 164, 122))
            : new SolidColorBrush(Color.FromArgb(20, 216, 59, 1));

        public SolidColorBrush StatusBadgeText => Status == "Active"
            ? new SolidColorBrush(Color.FromArgb(255, 10, 130, 96))
            : new SolidColorBrush(Color.FromArgb(255, 180, 40, 0));

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
        private readonly List<UserItem> _allUsers = new();
        private readonly ObservableCollection<UserItem> _displayedUsers = new();
        private UserItem? _editingUser;
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

            TxtRowCount.Text =
                $"Showing {_displayedUsers.Count} of {_allUsers.Count} client{(_allUsers.Count != 1 ? "s" : "")}";
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
                    Email = r.Username,          // Username maps to email column for now
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
            _editingUser = _allUsers.FirstOrDefault(u => u.UserId == userId);
            if (_editingUser == null) return;

            // TODO: Open an EditClient dialog (separate from AddNewClient)
            // For now re-use AddNewClient as a read-only preview stub
            await Task.CompletedTask;
        }

        // ─────────────────────────────────────────
        // MANAGE MODULES  (stub)
        // ─────────────────────────────────────────
        private void ManageModules_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement Manage Modules dialog/panel
        }

        // ─────────────────────────────────────────
        // DEACTIVATE
        // ─────────────────────────────────────────
        private void DeactivateUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
            int userId = (int)item.Tag;
            var user = _allUsers.FirstOrDefault(u => u.UserId == userId);
            if (user == null) return;

            // TODO: confirmation dialog
            user.Status = "Inactive";
            ApplyFilters();
            UpdateKpiCards();
        }

        // ─────────────────────────────────────────
        // DELETE
        // ─────────────────────────────────────────
        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
            int userId = (int)item.Tag;
            var user = _allUsers.FirstOrDefault(u => u.UserId == userId);
            if (user == null || user.Status != "Inactive") return;

            // TODO: confirmation dialog
            _allUsers.Remove(user);
            ApplyFilters();
            UpdateKpiCards();
        }

        // ─────────────────────────────────────────
        // OLD INLINE DIALOG HANDLER (removed — kept as tombstone)
        // UserDialog_PrimaryButtonClick → replaced by AddNewClient
        // ─────────────────────────────────────────
    }
}