using aesth_clic.Session;
using aesth_clic.Views.Roles.Doctor.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;

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
            ApplyTierVisibility();
            NavView.SelectedItem = NavView.MenuItems[0];
            ContentFrame.Navigate(typeof(PatientManagement));
        }

        private void ApplyTierVisibility()
        {
            string tier = (AppSession.Instance.CurrentClient?.Tier
                           ?? throw new InvalidOperationException("No active client in session."))
                          .ToLowerInvariant();

            // Cast all menu items once for clarity
            var patients = (NavigationViewItem)NavView.MenuItems[0]; // Patients
            var appointment = (NavigationViewItem)NavView.MenuItems[1]; // Appointment
            var procedure = (NavigationViewItem)NavView.MenuItems[2]; // Procedure
            var patientProcedures = (NavigationViewItem)NavView.MenuItems[3]; // Patient Procedures
            var serviceMenu = (NavigationViewItem)NavView.MenuItems[4]; // Service Menu

            switch (tier)
            {
                case "basic":
                    patients.Visibility = Visibility.Visible;
                    appointment.Visibility = Visibility.Collapsed;
                    procedure.Visibility = Visibility.Collapsed;
                    patientProcedures.Visibility = Visibility.Collapsed;
                    serviceMenu.Visibility = Visibility.Collapsed;
                    break;

                case "standard":
                    patients.Visibility = Visibility.Visible;
                    appointment.Visibility = Visibility.Collapsed;
                    procedure.Visibility = Visibility.Collapsed;
                    patientProcedures.Visibility = Visibility.Collapsed;
                    serviceMenu.Visibility = Visibility.Visible;
                    break;

                case "premium":
                    patients.Visibility = Visibility.Visible;
                    appointment.Visibility = Visibility.Visible;
                    procedure.Visibility = Visibility.Visible;
                    patientProcedures.Visibility = Visibility.Visible;
                    serviceMenu.Visibility = Visibility.Visible;
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unrecognized clinic tier: '{tier}'. Expected 'basic', 'standard', or 'premium'.");
            }
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