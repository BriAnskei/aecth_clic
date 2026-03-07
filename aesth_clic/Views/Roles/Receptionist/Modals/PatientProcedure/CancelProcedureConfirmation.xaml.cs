using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace aesth_clic.Views.Roles.Receptionist.Modals.PatientProcedure
{
    public sealed partial class CancelProcedureConfirmation : ContentDialog
    {
        public bool Confirmed { get; private set; } = false;

        public CancelProcedureConfirmation(
            string patientName,
            string initials,
            string avatarColor,
            string procedureName,
            string status,
            string statusColor,
            string cost)
        {
            InitializeComponent();

            PatientAvatar.Background = new SolidColorBrush(HexToColor(avatarColor));
            TxtInitials.Text = initials;
            TxtPatientName.Text = patientName;
            TxtProcedureName.Text = procedureName;
            TxtCost.Text = cost;

            TxtStatus.Text = status;
            TxtStatus.Foreground = new SolidColorBrush(HexToColor(statusColor));
            StatusBadge.Background = status switch
            {
                "Completed" => new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0xE8, 0xF5, 0xE9)),
                "Scheduled" => new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0xE3, 0xF2, 0xFD)),
                _ => new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0xFF, 0xF8, 0xE1)),
            };
        }

        private void OnConfirmClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
            => Confirmed = true;

        private static Windows.UI.Color HexToColor(string hex)
        {
            hex = hex.TrimStart('#');
            if (hex.Length == 6)
                return ColorHelper.FromArgb(
                    0xFF,
                    System.Convert.ToByte(hex[0..2], 16),
                    System.Convert.ToByte(hex[2..4], 16),
                    System.Convert.ToByte(hex[4..6], 16));
            return Colors.Gray;
        }
    }
}