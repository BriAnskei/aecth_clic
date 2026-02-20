using aesth_clic.Views;
using aesth_clic.Views.Roles.Doctor.Pages;
using aesth_clic.Views.Roles.Receptionist.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using PatientManagement = aesth_clic.Views.Roles.Receptionist.Pages.PatientManagement;
using PatientProcedures = aesth_clic.Views.Roles.Receptionist.Pages.PatientProcedures;

namespace aesth_clic.Views.Roles
{
    public sealed partial class ReceptionistShell : Page
    {
        public ReceptionistShell()
        {
            InitializeComponent();
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            NavView.SelectedItem = NavView.MenuItems[0];
            ContentFrame.Navigate(typeof(PatientManagement));
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item)
            {
                string tag = item.Tag?.ToString() ?? string.Empty;
                switch (tag)
                {
                    case "PatientManagement":
                        ContentFrame.Navigate(typeof(Receptionist.Pages.PatientManagement));
                        break;
                    case "Appointment":
                        ContentFrame.Navigate(typeof(Receptionist.Pages.AppointmentManagement));
                        break;
                    case "DoctorsAvailability":
                        ContentFrame.Navigate(typeof(DoctorsAvailability));
                        break;
                    case "ServiceMenu":
                        ContentFrame.Navigate(typeof(Receptionist.Pages.ServiceMenu));  // fully qualified
                        break;
                    case "PatientProcedure":
                        ContentFrame.Navigate(typeof(PatientProcedures));
                        break;
                    case "PaymentManagement":
                        ContentFrame.Navigate(typeof(PaymentManagement));
                        break;
                }
            }
        }

        private void OnLogout(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(RoleSelectionPage));
        }
    }
}