using Microsoft.UI.Xaml.Controls;

namespace aesth_clic.Views.Roles.SuperAdmin.Modals
{
    public sealed partial class ConfirmMarkAsPaid : ContentDialog
    {
        // Set to true only when the user clicks "Confirm Payment"
        public bool Confirmed { get; private set; } = false;

        public ConfirmMarkAsPaid(
            string clientName,
            string clinicName,
            string amount,
            string dueDate)
        {
            InitializeComponent();

            TextClientName.Text = clientName;
            TextClinicName.Text = clinicName;
            TextAmount.Text = amount;
            TextDueDate.Text = dueDate;
        }

        private void OnConfirmClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Confirmed = true;
            // No args.Cancel — let the dialog close naturally on primary click
        }
    }
}