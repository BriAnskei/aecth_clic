using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using aesth_clic.Views;
using aesth_clic.Views.Roles.Doctor.Pages;

namespace aesth_clic.Views.Roles
{
    public sealed partial class DoctorShell : Page
    {
        public DoctorShell()
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
                    case "Patients":
                        ContentFrame.Navigate(typeof(PatientManagement));
                        break;
                    case "Appointment":
                        ContentFrame.Navigate(typeof(AppointmentManagement));
                        break;
                    case "Procedure":
                        ContentFrame.Navigate(typeof(ProcedureManagement));
                        break;
                    case "PatientProcedures":
                        ContentFrame.Navigate(typeof(PatientProcedures));
                        break;
                    case "ServiceMenu":
                        ContentFrame.Navigate(typeof(ServiceMenu));
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