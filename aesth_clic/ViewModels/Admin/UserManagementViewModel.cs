using aesth_clic.Views.Roles.Admin.Pages;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Windows.UI;

namespace aesth_clic.ViewModels.Admin
{
    internal class UserManagementViewModel : INotifyPropertyChanged
    {
        private readonly List<StaffUserItem> _allUsers = new();
        public ObservableCollection<StaffUserItem> DisplayedUsers { get; } = new();

        // ──────────────────────────────────────────────────────
        // LOADING STATE
        // ──────────────────────────────────────────────────────
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        // ──────────────────────────────────────────────────────
        // FILTER STATE
        // ──────────────────────────────────────────────────────
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ApplyFilters(); }
        }

        private string _selectedRole = "All";
        public string SelectedRole
        {
            get => _selectedRole;
            set { _selectedRole = value; OnPropertyChanged(); ApplyFilters(); }
        }

        private string _selectedStatus = "All";
        public string SelectedStatus
        {
            get => _selectedStatus;
            set { _selectedStatus = value; OnPropertyChanged(); ApplyFilters(); }
        }

        // ──────────────────────────────────────────────────────
        // KPI COUNTERS  (always reflect _allUsers, not filtered)
        // ──────────────────────────────────────────────────────
        public int TotalUsers => _allUsers.Count;
        public int ActiveUsers => _allUsers.Count(u => u.Status == "Active");
        public int DeactivatedUsers => _allUsers.Count(u => u.Status == "Deactivated");

        // ──────────────────────────────────────────────────────
        // LOAD FROM DB  (maps DTO list → StaffUserItem list)
        // ──────────────────────────────────────────────────────
        public void LoadFromDb(IEnumerable<(string Id, string Name, string Email, string Phone, string Role, string Status, string Username)> users)
        {
            _allUsers.Clear();

            foreach (var u in users)
                _allUsers.Add(BuildItem(u.Id, u.Name, u.Email, u.Phone, u.Role, u.Status, u.Username));

            ApplyFilters();
        }

        // ──────────────────────────────────────────────────────
        // FACTORY HELPER  (mirrors the existing BuildItem logic)
        // ──────────────────────────────────────────────────────
        private static StaffUserItem BuildItem(
            string id, string name, string email, string phone,
            string role, string status, string username = "")
        {
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var initials = parts.Length >= 2
                ? $"{parts[0][0]}{parts[^1][0]}"
                : name.Length > 0 ? name[0].ToString() : "?";

            var avatarColor = role switch
            {
                "Doctor" => Color.FromArgb(255, 91, 45, 142),
                "Receptionist" => Color.FromArgb(255, 0, 120, 212),
                "Pharmacist" => Color.FromArgb(255, 230, 126, 34),
                _ => Color.FromArgb(255, 136, 136, 136),
            };

            var roleColor = role switch
            {
                "Doctor" => Color.FromArgb(255, 91, 45, 142),
                "Receptionist" => Color.FromArgb(255, 0, 120, 212),
                "Pharmacist" => Color.FromArgb(255, 230, 126, 34),
                _ => Color.FromArgb(255, 85, 85, 85),
            };

            string normalizedStatus = status.ToLower() == "active" ? "Active" : "Deactivated";

            return new StaffUserItem
            {
                UserId = id,
                FullName = name,
                Email = email,
                Phone = phone,
                Role = role,
                Status = normalizedStatus,
                Username = username,
                Initials = initials.ToUpper(),
                AvatarColor = new SolidColorBrush(avatarColor),
                RoleBadgeColor = new SolidColorBrush(roleColor),
            };
        }

        // ──────────────────────────────────────────────────────
        // FILTERS
        // ──────────────────────────────────────────────────────
        public void ApplyFilters()
        {
            var filtered = _allUsers.Where(u =>
                (string.IsNullOrEmpty(SearchText)
                    || u.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                    || u.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                    || u.Phone.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                && (SelectedRole == "All" || u.Role == SelectedRole)
                && (SelectedStatus == "All" || u.Status == SelectedStatus)
            ).ToList();

            DisplayedUsers.Clear();
            foreach (var u in filtered)
                DisplayedUsers.Add(u);

            // Notify KPIs so code-behind can refresh card text
            OnPropertyChanged(nameof(TotalUsers));
            OnPropertyChanged(nameof(ActiveUsers));
            OnPropertyChanged(nameof(DeactivatedUsers));
        }

        // ──────────────────────────────────────────────────────
        // CRUD HELPERS
        // ──────────────────────────────────────────────────────

        /// <summary>Returns the item from the master list, or null.</summary>
        public StaffUserItem? FindUser(string userId) =>
            _allUsers.FirstOrDefault(u => u.UserId == userId);

        /// <summary>Marks a user as Deactivated in-memory and re-filters.</summary>
        public void DeactivateUser(string userId)
        {
            var user = FindUser(userId);
            if (user is null) return;
            user.Status = "Deactivated";
            ApplyFilters();
        }

        /// <summary>Marks a user as Active in-memory and re-filters.</summary>
        public void ReactivateUser(string userId)
        {
            var user = FindUser(userId);
            if (user is null) return;
            user.Status = "Active";
            ApplyFilters();
        }

        /// <summary>Removes a deactivated user from the master list and re-filters.</summary>
        public void DeleteUser(string userId)
        {
            var user = FindUser(userId);
            if (user is null || user.Status != "Deactivated") return;
            _allUsers.Remove(user);
            ApplyFilters();
        }

        /// <summary>Replaces in-memory fields after a successful edit and re-filters.</summary>
        public void UpdateUser(string userId, string name, string email,
                               string phone, string role, string username)
        {
            var user = FindUser(userId);
            if (user is null) return;

            // Rebuild derived fields
            var updated = BuildItem(userId, name, email, phone, role, user.Status, username);

            user.FullName = updated.FullName;
            user.Email = updated.Email;
            user.Phone = updated.Phone;
            user.Role = updated.Role;
            user.Username = updated.Username;
            user.Initials = updated.Initials;
            user.AvatarColor = updated.AvatarColor;
            user.RoleBadgeColor = updated.RoleBadgeColor;

            ApplyFilters();
        }

        // ──────────────────────────────────────────────────────
        // INPC
        // ──────────────────────────────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}