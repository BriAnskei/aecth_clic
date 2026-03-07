using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace aesth_clic.Views.Roles.Doctor.Modals
{
    public sealed partial class ViewAppointmentDetails : ContentDialog
    {
        public ViewAppointmentDetails()
        {
            InitializeComponent();
        }

        public void Load(
            string patientName,
            string initials,
            SolidColorBrush avatarColor,
            string procedureName,
            string status,
            string appointmentDate,
            string procedureDate)
        {
            PatientAvatarBorder.Background = avatarColor;
            TxtInitials.Text = initials;
            TxtPatientName.Text = patientName;
            TxtProcedureName.Text = procedureName;
            TxtAppointmentDate.Text = appointmentDate;
            TxtProcedureDate.Text = string.IsNullOrEmpty(procedureDate) ? "Not set" : procedureDate;

            // Status badge colors
            var (label, fg, bg) = status.ToLower() switch
            {
                "completed" => ("Completed",
                    Color.FromArgb(0xFF, 0x2E, 0x7D, 0x32),
                    Color.FromArgb(0xFF, 0xE8, 0xF5, 0xE9)),
                "scheduled" => ("Scheduled",
                    Color.FromArgb(0xFF, 0x00, 0x78, 0xD4),
                    Color.FromArgb(0xFF, 0xE3, 0xF2, 0xFD)),
                "cancelled" => ("Cancelled",
                    Color.FromArgb(0xFF, 0xC5, 0x0F, 0x1F),
                    Color.FromArgb(0xFF, 0xFD, 0xEC, 0xEA)),
                _ => ("Pending",
                    Color.FromArgb(0xFF, 0xF5, 0x9E, 0x0B),
                    Color.FromArgb(0xFF, 0xFF, 0xF8, 0xE1)),
            };

            TxtStatus.Text = label;
            TxtStatus.Foreground = new SolidColorBrush(fg);
            StatusBadge.Background = new SolidColorBrush(bg);
        }
    }
}