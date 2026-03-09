using aesth_clic.Views.Roles.Receptionist.Pages;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;

namespace aesth_clic.Views.Roles.Receptionist.Modals
{
    // ── Row view-model for the medicines table ─────────────────────────────────
    public class MedicineRowItem
    {
        public string MedicineName { get; set; } = string.Empty;
        public string QuantityDisplay { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
    }

    // ── Modal ──────────────────────────────────────────────────────────────────
    public sealed partial class ViewPaymentDetails : ContentDialog
    {
        public ViewPaymentDetails(PaymentItem record)
        {
            InitializeComponent();
            Populate(record);
        }

        // ── Populate all UI fields from the PaymentItem ────────────────────────
        private void Populate(PaymentItem record)
        {
            // ── Patient header ────────────────────────────────────────────────
            PatientInitials.Text = record.Initials;
            PatientAvatar.Background = record.AvatarColor;
            PatientNameText.Text = record.PatientName;
            PatientSubText.Text = BuildSubText(record);

            // ── Procedure section ─────────────────────────────────────────────
            ProcedureNameText.Text = record.ProcedureName;
            ProcedureAmountText.Text = record.Amount;

            // ── Medicines section ─────────────────────────────────────────────
            var medicines = BuildMedicineRows(record);
            TxtMedicineCount.Text = medicines.Count.ToString();
            MedicineListControl.ItemsSource = medicines;
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private static string BuildSubText(PaymentItem record)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(record.DoctorName) && record.DoctorName != "Unassigned")
                parts.Add($"Dr. {record.DoctorName}");

            if (record.AppointmentDate != "—")
                parts.Add($"Appt: {record.AppointmentDate}");

            if (record.ProcedureDate != "—")
                parts.Add($"Procedure: {record.ProcedureDate}");

            return string.Join("  ·  ", parts);
        }

        private static List<MedicineRowItem> BuildMedicineRows(PaymentItem record)
        {
            var rows = new List<MedicineRowItem>();

            foreach (var m in record.Medicines)
            {
                rows.Add(new MedicineRowItem
                {
                    MedicineName = m.MedicineName,
                    QuantityDisplay = m.Quantity.ToString(),
                    Unit = m.Unit,
                });
            }

            return rows;
        }
    }
}