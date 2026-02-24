using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using aesth_clic.Models.Users;
using aesth_clic.Views.Roles.SuperAdmin.Pages;

namespace aesth_clic.ViewModels.SuperAdmin
{
    internal class UserManagementViewModel : INotifyPropertyChanged
    {
        private readonly List<UserItem> _allUsers = new();
        public ObservableCollection<UserItem> DisplayedUsers { get; } = new();
        private int _nextId = 1;

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

        public int TotalUsers => _allUsers.Count;
        public int ActiveUsers => _allUsers.Count(u => u.Status == "Active");
        public int DeactivatedUsers => _allUsers.Count(u => u.Status == "Deactivated");

        public void LoadFromDb(List<AdminClients> clients)
        {
            _allUsers.Clear();
            _nextId = 1;

            foreach (var client in clients)
            {
                if (client.User is null)
                    continue;

                string rawStatus = client.Company?.status ?? "active";
                string status = rawStatus.ToLower() == "active" ? "Active" : "Deactivated";
                string tier = client.Company?.module_tier ?? "basic";

                _allUsers.Add(
                    new UserItem
                    {
                        UserId = client.User.Id,
                        CompanyId = client.Company?.id ?? 0,
                        FullName = client.User.FullName ?? string.Empty,
                        Email = client.User.Email ?? string.Empty,
                        Phone = client.User.PhoneNumber ?? string.Empty,
                        ClinicName = client.Company?.name ?? string.Empty,
                        Status = status,
                        Tier = tier,
                        Password = client.User.Password ?? string.Empty,
                        Username = client.User.Username ?? string.Empty, // ← ADDED
                    }
                );

                if (client.User.Id >= _nextId)
                    _nextId = client.User.Id + 1;
            }

            ApplyFilters();
        }

        public void ApplyFilters()
        {
            DisplayedUsers.Clear();

            var filtered = _allUsers.Where(u =>
                (
                    string.IsNullOrEmpty(SearchText)
                    || u.FullName.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase)
                    || u.Email.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase)
                    || u.ClinicName.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase)
                )
                && (SelectedStatus == "All" || u.Status == SelectedStatus)
                && (SelectedTier == "All" || u.Tier == SelectedTier)
            );

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

        public UserItem? FindUser(int userId) => _allUsers.FirstOrDefault(u => u.UserId == userId);

        public void AddUser(UserItem user)
        {
            user.UserId = _nextId++;
            _allUsers.Add(user);
            ApplyFilters();
        }

        public void DeactivateUser(int userId)
        {
            var user = _allUsers.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return;
            user.Status = "Deactivated";
            ApplyFilters();
        }

        public void ReactivateUser(int userId)
        {
            var user = _allUsers.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return;
            user.Status = "Active";
            ApplyFilters();
        }

        public void DeleteUser(int userId)
        {
            var user = _allUsers.FirstOrDefault(u => u.UserId == userId);
            if (user == null || user.Status != "Deactivated")
                return;
            _allUsers.Remove(user);
            ApplyFilters();
        }

        public void UpdateUserTier(int userId, string newTier)
        {
            var user = _allUsers.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return;
            user.Tier = newTier;
            ApplyFilters();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
