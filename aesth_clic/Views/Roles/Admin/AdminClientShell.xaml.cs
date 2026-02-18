using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using aesth_clic.Views;
using aesth_clic.Views.Roles.Admin.Pages;

namespace aesth_clic.Views.Roles
{
    public sealed partial class AdminClientShell : Page
    {
        public AdminClientShell()
        {
            InitializeComponent();
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            NavView.SelectedItem = NavView.MenuItems[0];
            ContentFrame.Navigate(typeof(AdminDashboard));
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item)
            {
                string tag = item.Tag?.ToString() ?? string.Empty;
                switch (tag)
                {
                    case "Dashboard":
                        ContentFrame.Navigate(typeof(AdminDashboard));
                        break;
                    case "UsersManagement":
                        ContentFrame.Navigate(typeof(UserManagement));
                        break;
                    case "TermsAndConditions":
                        ContentFrame.Navigate(typeof(TermsAndConditions));
                        break;
                    default:
                        ContentFrame.Navigate(typeof(BlankPage), tag);
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