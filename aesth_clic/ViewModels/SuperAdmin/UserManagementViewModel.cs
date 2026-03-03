using aesth_clic.Master.Dto;
using aesth_clic.Views.Roles.SuperAdmin.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace aesth_clic.ViewModels.SuperAdmin
{
    internal class UserManagementViewModel : INotifyPropertyChanged
    {
        private readonly List<UserItem> _allUsers = new();
        public ObservableCollection<UserItem> DisplayedUsers { get; } = new();

        // ──────────────────────────────────────────────────────
        // LOADING STATE
        // ──────────────────────────────────────────────────────
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ApplyFilters(); }
        }

        private string _selectedStatus = "All";
        public string SelectedStatus
        {
            get => _selectedStatus;
            set { _selectedStatus = value; OnPropertyChanged(); ApplyFilters(); }
        }

        private string _selectedTier = "All";
        public string SelectedTier
        {
            get => _selectedTier;
            set { _selectedTier = value; OnPropertyChanged(); ApplyFilters(); }
        }

        public int TotalUsers => _allUsers.Count;
        public int ActiveUsers => _allUsers.Count(u => u.Status == "Active");
        public int DeactivatedUsers => _allUsers.Count(u => u.Status == "Deactivated");

        // ──────────────────────────────────────────────────────
        // LOAD FROM DB  (maps AdminClinicDetailsDto → UserItem)
        // ──────────────────────────────────────────────────────
        public void LoadFromDb(List<AdminClinicDetailsDto> clinics)
        {
            _allUsers.Clear();

            foreach (var dto in clinics)
            {
                string status = dto.Status.ToLower() == "active" ? "Active" : "Deactivated";

                _allUsers.Add(new UserItem
                {
                    UserId = dto.UserId,
                    CompanyId = dto.ClientId,
                    ClinicCode = dto.ClinicCode,
                    FullName = dto.FullName,
                    Email = dto.Email,
                    Phone = dto.PhoneNumber,
                    Username = dto.Username,
                    ClinicName = dto.ClinicName,
                    Tier = dto.Tier,
                    Status = status,
                });
            }

            ApplyFilters();
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
                    || u.ClinicName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                && (SelectedStatus == "All" || u.Status == SelectedStatus)
                && (SelectedTier == "All" || u.Tier == SelectedTier)
            ).ToList();

            DisplayedUsers.Clear();

            for (int i = 0; i < filtered.Count; i++)
            {
                filtered[i].RowNumber = i + 1;
                DisplayedUsers.Add(filtered[i]);
            }

            OnPropertyChanged(nameof(TotalUsers));
            OnPropertyChanged(nameof(ActiveUsers));
            OnPropertyChanged(nameof(DeactivatedUsers));
        }

        // ──────────────────────────────────────────────────────
        // CRUD HELPERS
        // ──────────────────────────────────────────────────────
        public UserItem? FindUser(int userId) =>
            _allUsers.FirstOrDefault(u => u.UserId == userId);

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

        public void UpdateUserTier(int userId, string newTier)
        {
            var user = _allUsers.FirstOrDefault(u => u.UserId == userId);
            if (user == null) return;
            user.Tier = newTier;
            ApplyFilters();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}