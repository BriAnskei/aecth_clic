using aesth_clic.Views.Roles.SuperAdmin.Pages;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace aesth_clic.ViewModels.SuperAdmin
{
    internal class UserManagementViewModel : INotifyPropertyChanged
    {
        // ─────────────────────────────────────────
        // STATE CONTAINERS
        // ─────────────────────────────────────────

        private readonly List<UserItem> _allUsers = new();

        public ObservableCollection<UserItem> DisplayedUsers { get; } = new();

        private int _nextId = 1;

        // ─────────────────────────────────────────
        // FILTER STATE
        // ─────────────────────────────────────────

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        private string _selectedStatus = "All";
        public string SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                _selectedStatus = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        // ─────────────────────────────────────────
        // KPI STATE
        // ─────────────────────────────────────────

        public int TotalUsers => _allUsers.Count;
        public int ActiveUsers => _allUsers.Count(u => u.Status == "Active");
        public int DeactivatedUsers => _allUsers.Count(u => u.Status == "Deactivated");

        // ─────────────────────────────────────────
        // LOAD DATA (Mock for now)
        // ─────────────────────────────────────────

        public void LoadMockData()
        {
            _allUsers.Clear();
            _nextId = 1;

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

            ApplyFilters();
        }

        // ─────────────────────────────────────────
        // FILTERING
        // ─────────────────────────────────────────

        public void ApplyFilters()
        {
            DisplayedUsers.Clear();

            var filtered = _allUsers.Where(u =>
                (string.IsNullOrEmpty(SearchText)
                    || u.FullName.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase)
                    || u.Email.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase)
                    || u.ClinicName.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase))
                &&
                (SelectedStatus == "All" || u.Status == SelectedStatus));

            int row = 1;
            foreach (var user in filtered)
            {
                user.RowNumber = row++;
                DisplayedUsers.Add(user);
            }

            OnPropertyChanged(nameof(TotalUsers));
            OnPropertyChanged(nameof(ActiveUsers));
            OnPropertyChanged(nameof(DeactivatedUsers));
        }

        // ─────────────────────────────────────────
        // LOOKUP HELPER  (used by code-behind)
        // ─────────────────────────────────────────

        /// <summary>
        /// Returns the UserItem with the given ID from the full (unfiltered) list,
        /// or null if not found.
        /// </summary>
        public UserItem? FindUser(int userId)
            => _allUsers.FirstOrDefault(u => u.UserId == userId);

        // ─────────────────────────────────────────
        // STATE MUTATIONS
        // ─────────────────────────────────────────

        public void AddUser(UserItem user)
        {
            user.UserId = _nextId++;
            _allUsers.Add(user);
            ApplyFilters();
        }

        public void DeactivateUser(int userId)
        {
            var user = _allUsers.FirstOrDefault(u => u.UserId == userId);
            if (user == null) return;

            user.Status = "Deactivated";
            ApplyFilters();
        }

        public void ReactivateUser(int userId)
        {
            var user = _allUsers.FirstOrDefault(u => u.UserId == userId);
            if (user == null) return;

            user.Status = "Active";
            ApplyFilters();
        }

        public void DeleteUser(int userId)
        {
            var user = _allUsers.FirstOrDefault(u => u.UserId == userId);
            if (user == null || user.Status != "Deactivated") return;

            _allUsers.Remove(user);
            ApplyFilters();
        }

        // ─────────────────────────────────────────
        // NOTIFY
        // ─────────────────────────────────────────

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}