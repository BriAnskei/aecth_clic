using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using Windows.UI;

namespace aesth_clic.Views.Roles.Receptionist.Pages
{
    // ── Medicine sub-model (flat, for display) ─────────────────────────────────
    public class MedicineLineItem
    {
        public string MedicineName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
    }

    // ── UI display model ───────────────────────────────────────────────────────
    public class PaymentItem
    {
        public string PaymentId { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public SolidColorBrush AvatarColor { get; set; } = new(Color.FromArgb(255, 91, 45, 142));
        public string ProcedureName { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        /// <summary>Raw DB value (lowercase) — used for controller calls.</summary>
        public string RawStatus { get; set; } = string.Empty;
        /// <summary>Title-cased value for display (e.g. "Pending", "Completed").</summary>
        public string Status { get; set; } = string.Empty;
        public SolidColorBrush StatusForeground { get; set; } = new(Color.FromArgb(255, 91, 45, 142));
        public string DoctorName { get; set; } = string.Empty;
        public string AppointmentDate { get; set; } = "—";
        public string ProcedureDate { get; set; } = "—";

        /// <summary>Medicines from the linked Prescription (always populated for Completed payments).</summary>
        public List<MedicineLineItem> Medicines { get; set; } = new();
    }
}