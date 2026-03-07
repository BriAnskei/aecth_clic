using aesth_clic.Views.Roles.Doctor.Pages;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.UI;

namespace aesth_clic.ViewModels.Doctor
{
    internal class PatientManagementViewModel : INotifyPropertyChanged
    {
        private readonly List<PatientItem> _allPatients = new();
        public ObservableCollection<PatientItem> DisplayedPatients { get; } = new();

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

        private string _selectedGender = "All";
        public string SelectedGender
        {
            get => _selectedGender;
            set { _selectedGender = value; OnPropertyChanged(); ApplyFilters(); }
        }

        private string _selectedSort = "AZ";
        public string SelectedSort
        {
            get => _selectedSort;
            set { _selectedSort = value; OnPropertyChanged(); ApplyFilters(); }
        }

        // ──────────────────────────────────────────────────────
        // KPI COUNTERS  (reflect DisplayedPatients — i.e. filtered)
        // ──────────────────────────────────────────────────────
        public int TotalPatients => DisplayedPatients.Count;
        public int MalePatients => DisplayedPatients.Count(p => p.Gender == "Male");
        public int FemalePatients => DisplayedPatients.Count(p => p.Gender == "Female");

        // ──────────────────────────────────────────────────────
        // LOAD FROM DB  (maps DTO list → PatientItem list)
        // ──────────────────────────────────────────────────────
        public void LoadFromDb(IEnumerable<(
            string PatientId,
            string FullName,
            string Email,
            string Phone,
            string Gender,
            int Age,
            string Address)> patients)
        {
            _allPatients.Clear();

            foreach (var p in patients)
                _allPatients.Add(BuildItem(
                    p.PatientId, p.FullName, p.Email,
                    p.Phone, p.Gender, p.Age, p.Address));

            ApplyFilters();
        }

        // ──────────────────────────────────────────────────────
        // FACTORY HELPER
        // ──────────────────────────────────────────────────────
        public static PatientItem BuildItem(
            string id, string name, string email,
            string phone, string gender, int age, string address)
        {
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var initials = parts.Length >= 2
                ? $"{parts[0][0]}{parts[^1][0]}"
                : name.Length > 0 ? name[0].ToString() : "?";

            var avatarColor = gender switch
            {
                "Female" => Color.FromArgb(255, 194, 24, 91),
                "Male" => Color.FromArgb(255, 0, 120, 212),
                _ => Color.FromArgb(255, 91, 45, 142),
            };

            var genderBadgeBg = gender switch
            {
                "Female" => Color.FromArgb(255, 252, 228, 236),
                "Male" => Color.FromArgb(255, 227, 242, 253),
                _ => Color.FromArgb(255, 237, 228, 249),
            };

            var genderBadgeFg = gender switch
            {
                "Female" => Color.FromArgb(255, 194, 24, 91),
                "Male" => Color.FromArgb(255, 0, 120, 212),
                _ => Color.FromArgb(255, 91, 45, 142),
            };

            return new PatientItem
            {
                PatientId = id,
                FullName = name,
                Email = email,
                Phone = phone,
                Gender = gender,
                Age = age,
                Address = address,
                Initials = initials.ToUpper(),
                AvatarColor = new SolidColorBrush(avatarColor),
                GenderBadgeColor = new SolidColorBrush(genderBadgeBg),
                GenderBadgeForeground = new SolidColorBrush(genderBadgeFg),
            };
        }

        // ──────────────────────────────────────────────────────
        // FILTERS + SORT
        // ──────────────────────────────────────────────────────
        public void ApplyFilters()
        {
            var filtered = _allPatients.Where(p =>
                (string.IsNullOrEmpty(SearchText)
                    || p.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                    || p.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                    || p.Phone.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                && (SelectedGender == "All" || p.Gender == SelectedGender)
            );

            filtered = SelectedSort == "ZA"
                ? filtered.OrderByDescending(p => p.FullName)
                : filtered.OrderBy(p => p.FullName);

            DisplayedPatients.Clear();
            foreach (var p in filtered)
                DisplayedPatients.Add(p);

            OnPropertyChanged(nameof(TotalPatients));
            OnPropertyChanged(nameof(MalePatients));
            OnPropertyChanged(nameof(FemalePatients));
        }

        // ──────────────────────────────────────────────────────
        // CRUD HELPERS
        // ──────────────────────────────────────────────────────

        /// <summary>Returns the item from the master list, or null.</summary>
        public PatientItem? FindPatient(string patientId) =>
            _allPatients.FirstOrDefault(p => p.PatientId == patientId);

        /// <summary>Removes a patient from the master list and re-filters.</summary>
        public void DeletePatient(string patientId)
        {
            var patient = FindPatient(patientId);
            if (patient is null) return;
            _allPatients.Remove(patient);
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