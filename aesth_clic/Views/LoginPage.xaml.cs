using aesth_clic.Controller;
using aesth_clic.Services;
using aesth_clic.Views.Roles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using WinRT.Interop;

namespace aesth_clic.Views
{
    public sealed partial class LoginPage : Page
    {
      
  

        public LoginPage()
        {
            InitializeComponent();
       
        }

        private async void OnEnterPressed(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
                await TryLogin();
        }

        private async void OnLoginClicked(object sender, RoutedEventArgs e)
        {
            await TryLogin();
        }

        private async Task TryLogin()
        {
            var username = UsernameBox.Text?.Trim();
            var password = PasswordBox.Password;
            var selectedItem = RoleComboBox.SelectedItem as ComboBoxItem;
            var role = selectedItem?.Tag?.ToString();



            // Validate fields
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(role))
            {
                ShowError("Please fill in all fields.");
                return;
            }

            SetLoadingState(true);

       


            SetLoadingState(false);


     

            // Success — maximize then navigate
            MaximizeWindow();
            HideError();
            NavigateByRole();

        }

        private void NavigateByRole
            ()
        {
            string role = UserSession.CurrentUser?.Role ?? "";

            switch (role)
            {
                case "super_admin": Frame.Navigate(typeof(SuperAdminShell)); break;
                case "admin": Frame.Navigate(typeof(AdminClientShell)); break;
                case "doctor": Frame.Navigate(typeof(DoctorShell)); break;
                case "receptionist": Frame.Navigate(typeof(ReceptionistShell)); break;
                case "pharmacist": Frame.Navigate(typeof(PharmacistShell)); break;
            }
        }

        private void SetLoadingState(bool isLoading)
        {
            LoginButton.IsEnabled = !isLoading;

            LoginButton.IsEnabled = !isLoading;
            LoginButtonText.Visibility = isLoading ? Visibility.Collapsed : Visibility.Visible;
            LoginLoadingPanel.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;

    
            UsernameBox.IsEnabled = !isLoading;
            PasswordBox.IsEnabled = !isLoading;
            RoleComboBox.IsEnabled = !isLoading;

        }




        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }

        private void HideError()
        {
            ErrorText.Visibility = Visibility.Collapsed;
        }

        [SupportedOSPlatform("windows10.0.17763.0")]
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