using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using aesth_clic.Views;

namespace aesth_clic.Views.Roles
{
    public sealed partial class PharmacistShell : Page
    {
        public PharmacistShell()
        {
            InitializeComponent();
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            NavView.SelectedItem = NavView.MenuItems[0];
            ContentFrame.Navigate(typeof(BlankPage), "Medicine Inventory");
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item)
            {
                ContentFrame.Navigate(typeof(BlankPage), item.Content?.ToString());
            }
        }

        private void OnLogout(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(RoleSelectionPage));
        }
    }
}