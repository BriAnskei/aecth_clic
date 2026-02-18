using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using aesth_clic.Views.Roles;
using Microsoft.UI.Windowing;
using Microsoft.UI;
using WinRT.Interop;
using aesth_clic;  // ← add this so App.MainWindow is visible

namespace aesth_clic.Views
{
    public sealed partial class RoleSelectionPage : Page
    {
        public RoleSelectionPage()
        {
            InitializeComponent();
        }

        private void OnRoleSelected(object sender, RoutedEventArgs e)
        {
            MaximizeWindow();

            var button = sender as Button;
            var role = button?.Tag?.ToString();

            switch (role)
            {
                case "SuperAdmin": Frame.Navigate(typeof(SuperAdminShell)); break;
                case "AdminClient": Frame.Navigate(typeof(AdminClientShell)); break;
                case "Doctor": Frame.Navigate(typeof(DoctorShell)); break;
                case "Receptionist": Frame.Navigate(typeof(ReceptionistShell)); break;
                case "Pharmacist": Frame.Navigate(typeof(PharmacistShell)); break;
            }
        }

        private static void MaximizeWindow()
        {
            var window = (Application.Current as App)?.MainWindow;
            if (window == null) return;

            var hwnd = WindowNative.GetWindowHandle(window);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            if (appWindow.Presenter is OverlappedPresenter presenter)
                presenter.Maximize();
        }
    }
}