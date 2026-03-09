using aesth_clic.Tenant.Model;
using aesth_clic.Views.Roles.Pharmacist.Pages;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace aesth_clic.ViewModels.Pharmacist
{
    internal class PatientMedicineViewModel : INotifyPropertyChanged
    {
        private readonly List<PatientMedicineItem> _allItems = new();

        // Keep source prescriptions so ViewDetails_Click can retrieve the full object
        private readonly List<Prescription> _allPrescriptions = new();

        public ObservableCollection<PatientMedicineItem> DisplayedItems { get; } = new();

        // ──────────────────────────────────────────────────────
        // FILTER STATE
        // ──────────────────────────────────────────────────────
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ApplyFilters(); }
        }

        // ──────────────────────────────────────────────────────
        // KPI COUNTER
        // ──────────────────────────────────────────────────────
        public int TotalPatients => _allItems.Count;

        // ──────────────────────────────────────────────────────
        // LOAD FROM DB
        // ──────────────────────────────────────────────────────
        public void LoadFromDb(IEnumerable<Prescription> prescriptions)
        {
            _allItems.Clear();
            _allPrescriptions.Clear();

            foreach (var p in prescriptions)
            {
                _allPrescriptions.Add(p);

                var patient = p.PatientProcedure!.Patient;
                var patientName = patient.FullName;
                var gender = patient.Gender;

                var parts = patientName.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
                var initials = parts.Length >= 2
                    ? $"{parts[0][0]}{parts[^1][0]}"
                    : patientName.Length > 0 ? patientName[0].ToString() : "?";

                var avatarColor = gender.ToLower() switch
                {
                    "female" => "#C2185B",
                    "male" => "#0078D4",
                    _ => "#5B2D8E"
                };

                _allItems.Add(new PatientMedicineItem
                {
                    PatientId = patient.Id.ToString(),
                    PatientName = patientName,
                    PatientGender = gender,
                    Initials = initials.ToUpper(),
                    AvatarColor = avatarColor,
                    AssignedDoctor = p.PatientProcedure.User?.FullName ?? "Unassigned",
                    TotalMedicine = p.PatientMedicines.Count,
                });
            }

            ApplyFilters();
            OnPropertyChanged(nameof(TotalPatients));
        }

        // ──────────────────────────────────────────────────────
        // FILTERS
        // ──────────────────────────────────────────────────────
        public void ApplyFilters()
        {
            var filtered = _allItems
                .Where(p =>
                    string.IsNullOrEmpty(SearchText)
                    || p.PatientName.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase)
                    || p.AssignedDoctor.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p.PatientName);

            DisplayedItems.Clear();
            foreach (var p in filtered)
                DisplayedItems.Add(p);

            OnPropertyChanged(nameof(DisplayedItems));
        }

        // ──────────────────────────────────────────────────────
        // LOOKUP HELPERS
        // ──────────────────────────────────────────────────────
        /// <summary>Returns the display item from the master list, or null.</summary>
        public PatientMedicineItem? FindItem(string patientId) =>
            _allItems.FirstOrDefault(p => p.PatientId == patientId);

        /// <summary>Returns the full Prescription for a given patient ID, or null.</summary>
        public Prescription? FindPrescription(string patientId) =>
            _allPrescriptions.FirstOrDefault(p =>
                p.PatientProcedure!.Patient.Id.ToString() == patientId);

        // ──────────────────────────────────────────────────────
        // INPC
        // ──────────────────────────────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}