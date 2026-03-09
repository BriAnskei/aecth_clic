using aesth_clic.Tenant.Model;
using aesth_clic.Views.Roles.Receptionist.Pages;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.UI;

namespace aesth_clic.ViewModels.Receptionist
{
    internal class PaymentManagementViewModel : INotifyPropertyChanged
    {
        private readonly List<PaymentItem> _allPayments = new();
        public ObservableCollection<PaymentItem> DisplayedPayments { get; } = new();

        // ──────────────────────────────────────────────────────
        // FILTER STATE
        // ──────────────────────────────────────────────────────
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

        // ──────────────────────────────────────────────────────
        // LOAD FROM DB  (maps ProcedurePayment list → PaymentItem list)
        // ──────────────────────────────────────────────────────
        public void LoadFromDb(IEnumerable<ProcedurePayment> payments)
        {
            _allPayments.Clear();

            foreach (var p in payments)
            {
                var item = BuildItem(p);
                if (item is not null)
                    _allPayments.Add(item);
            }

            ApplyFilters();
        }

        // ──────────────────────────────────────────────────────
        // FACTORY HELPER
        // ──────────────────────────────────────────────────────
        public static PaymentItem? BuildItem(ProcedurePayment payment)
        {
            // Guard: navigation properties must be loaded
            var procedure = payment.PatientProcedure;
            if (procedure is null) return null;

            var patient = procedure.Patient;
            var service = procedure.ServiceMenu ?? payment.ServiceMenu;
            var doctor = procedure.User;

            var patientName = patient?.FullName ?? "Unknown Patient";
            var gender = patient?.Gender ?? string.Empty;

            // Initials
            var parts = patientName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var initials = parts.Length >= 2
                ? $"{parts[0][0]}{parts[^1][0]}"
                : patientName.Length > 0 ? patientName[0].ToString() : "?";

            // Avatar color by gender
            var avatarColor = gender switch
            {
                "Female" => Color.FromArgb(255, 194, 24, 91),
                "Male" => Color.FromArgb(255, 0, 120, 212),
                _ => Color.FromArgb(255, 91, 45, 142),
            };

            // Status — normalize to Title-case
            var rawStatus = payment.Status ?? string.Empty;
            var displayStatus = rawStatus.Length > 0
                ? char.ToUpper(rawStatus[0]) + rawStatus[1..].ToLower()
                : "Unknown";

            var statusFg = displayStatus switch
            {
                "Completed" => Color.FromArgb(255, 46, 125, 50),   // green
                "Pending" => Color.FromArgb(255, 245, 158, 11),   // amber
                _ => Color.FromArgb(255, 102, 102, 102),  // grey
            };

            // Amount from ServiceMenu.Price
            var price = service?.Price ?? 0.0;
            var amount = $"₱{price:N2}";

            // Doctor name
            var doctorName = doctor is not null
                ? doctor.FullName ?? $"Doctor #{doctor.Id}"
                : "Unassigned";

            // Dates
            var appointmentDate = procedure.AppointmentDate.HasValue
                ? procedure.AppointmentDate.Value.ToString("MMM dd, yyyy")
                : "—";
            var procedureDate = procedure.ProcedureDate.HasValue
                ? procedure.ProcedureDate.Value.ToString("MMM dd, yyyy")
                : "—";

            // ── Medicines from Prescription ───────────────────────────────────
            var medicines = new List<MedicineLineItem>();
            var prescription = procedure.Prescription;
            if (prescription?.PatientMedicines is not null)
            {
                foreach (var pm in prescription.PatientMedicines)
                {
                    if (pm.Medicine is null) continue;
                    medicines.Add(new MedicineLineItem
                    {
                        MedicineName = pm.Medicine.Name,
                        Quantity = pm.Quantity,
                        Unit = pm.Medicine.Unit,
                    });
                }
            }

            return new PaymentItem
            {
                PaymentId = payment.Id.ToString(),
                PatientId = (patient?.Id ?? 0).ToString(),
                PatientName = patientName,
                Initials = initials.ToUpper(),
                AvatarColor = new SolidColorBrush(avatarColor),
                ProcedureName = service?.Name ?? "Unknown Procedure",
                Amount = amount,
                RawStatus = rawStatus,
                Status = displayStatus,
                StatusForeground = new SolidColorBrush(statusFg),
                DoctorName = doctorName,
                AppointmentDate = appointmentDate,
                ProcedureDate = procedureDate,
                Medicines = medicines,
            };
        }

        // ──────────────────────────────────────────────────────
        // FILTERS
        // ──────────────────────────────────────────────────────
        public void ApplyFilters()
        {
            var filtered = _allPayments.Where(p =>
                (string.IsNullOrEmpty(SearchText)
                    || p.PatientName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                    || p.ProcedureName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                && (SelectedStatus == "All" || p.Status == SelectedStatus)
            );

            DisplayedPayments.Clear();
            foreach (var p in filtered)
                DisplayedPayments.Add(p);

            OnPropertyChanged(nameof(TotalPayments));
            OnPropertyChanged(nameof(PendingPayments));
            OnPropertyChanged(nameof(CompletedPayments));
        }

        // ──────────────────────────────────────────────────────
        // CRUD HELPERS
        // ──────────────────────────────────────────────────────
        public PaymentItem? FindPayment(string paymentId) =>
            _allPayments.FirstOrDefault(p => p.PaymentId == paymentId);

        // Counters (from filtered list)
        public int TotalPayments => DisplayedPayments.Count;
        public int PendingPayments => DisplayedPayments.Count(p => p.Status == "Pending");
        public int CompletedPayments => DisplayedPayments.Count(p => p.Status == "Completed");

        // ──────────────────────────────────────────────────────
        // INPC
        // ──────────────────────────────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}