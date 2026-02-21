using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using aesth_clic.Views;
using aesth_clic.Views.Roles.SuperAdmin.Pages;

namespace aesth_clic.Views.Roles
{
    public sealed partial class SuperAdminShell : Page
    {
        public SuperAdminShell()
        {
            InitializeComponent();
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            NavView.SelectedItem = NavView.MenuItems[0];
            ContentFrame.Navigate(typeof(Dashboard));
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item)
            {
                switch (item.Tag?.ToString())
                {
                    case "Dashboard":
                        ContentFrame.Navigate(typeof(Dashboard));
                        break;
                    case "UserManagement":
                        ContentFrame.Navigate(typeof(UserManagement));   // ← wired up
                        break;
                    case "TNC":
                        ContentFrame.Navigate(typeof(TNC));
                        break;
                    case "PaymentManagement":
                        ContentFrame.Navigate(typeof(PaymentManagement));
                        break;
                }
            }
        }

        private void OnLogout(object sender, TappedRoutedEventArgs e)
        {
            var window = (Application.Current as App)?.MainWindow;
            if (window != null)
            {
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

                if (appWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
                    presenter.Restore();
            }

            //Frame.Navigate(typeof(RoleSelectionPage));
            Frame.Navigate(typeof(LoginPage));
        }
    }
}