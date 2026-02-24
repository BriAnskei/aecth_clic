using aesth_clic.Models.Users;
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

        // ── NEW: Tier filter ─────────────────────
        private string _selectedTier = "All";
        public string SelectedTier
        {
            get => _selectedTier;
            set
            {
                _selectedTier = value;
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
        // LOAD FROM DB
        // ─────────────────────────────────────────

        /// <summary>
        /// Replaces the in-memory list with data fetched from the database.
        /// Call this on initial load and after any add/edit/delete operation.
        /// </summary>
        public void LoadFromDb(List<AdminClients> clients)
        {
            _allUsers.Clear();
            _nextId = 1;

            foreach (var client in clients)
            {
                if (client.User is null) continue;

                // Normalize status: DB stores "active"/"inactive", UI uses "Active"/"Deactivated"
                string rawStatus = client.Company?.status ?? "active";
                string status = rawStatus.ToLower() == "active" ? "Active" : "Deactivated";

                // Tier: DB stores "basic"/"standard"/"premium" — kept lowercase to match filter tags
                string tier = client.Company?.module_tier ?? "basic";

                _allUsers.Add(new UserItem
                {
                    UserId = client.User.Id,
                    FullName = client.User.FullName ?? string.Empty,
                    Email = client.User.Email ?? string.Empty,
                    Phone = client.User.PhoneNumber ?? string.Empty,
                    ClinicName = client.Company?.name ?? string.Empty,
                    Status = status,
                    Tier = tier,                   // ← NEW
                    Password = client.User.Password ?? string.Empty,
                });

                // Keep _nextId ahead of the highest existing ID
                if (client.User.Id >= _nextId)
                    _nextId = client.User.Id + 1;
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
                // Search: name, email, or clinic
                (string.IsNullOrEmpty(SearchText)
                    || u.FullName.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase)
                    || u.Email.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase)
                    || u.ClinicName.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase))
                &&
                // Status filter
                (SelectedStatus == "All" || u.Status == SelectedStatus)
                &&
                // Tier filter ← NEW
                (SelectedTier == "All" || u.Tier == SelectedTier));

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