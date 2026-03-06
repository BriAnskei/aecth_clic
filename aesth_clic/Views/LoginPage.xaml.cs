using aesth_clic.Controller;

using aesth_clic.Session;
using aesth_clic.Views.Roles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using WinRT.Interop;

namespace aesth_clic.Views
{
    public sealed partial class LoginPage : Page
    {
        private readonly AuthController _authController;

        public LoginPage()
        {
            InitializeComponent();

            // Initialize AuthController with AuthService
            _authController = App.Services.GetRequiredService<AuthController>();

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
            var clinicCode = ClinicCodeBox.Text?.Trim();

            // Validate fields
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(clinicCode))
            {
                ShowError("Please fill in all fields.");
                return;
            }

            SetLoadingState(true);
            HideError();

            try
            {
                // Call authentication backend
                // Note: AuthService already handles AppSession.Instance.Login() internally
                var user = await _authController.LoginAsync(clinicCode, username, password);

                if (user != null && AppSession.Instance.IsLoggedIn)
                {
                    SetLoadingState(false);

                    // Success — maximize then navigate
                    MaximizeWindow();
                    NavigateByRole();
                }
                else
                {
                    SetLoadingState(false);
                    ShowError("Login failed. Please try again.");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                SetLoadingState(false);
                ShowError(ex.Message);
            }
            catch (Exception ex)
            {
                SetLoadingState(false);
                ShowError(ex.Message);
            }
        }

        private void NavigateByRole()
        {
            string role = AppSession.Instance.CurrentUser?.Role ?? "";

            switch (role.ToLower())
            {
                case "super_admin":
                    Frame.Navigate(typeof(SuperAdminShell));
                    break;
                case "admin":
                    Frame.Navigate(typeof(AdminClientShell));
                    break;
                case "doctor":
                    Frame.Navigate(typeof(DoctorShell));
                    break;
                case "receptionist":
                    Frame.Navigate(typeof(ReceptionistShell));
                    break;
                case "pharmacist":
                    Frame.Navigate(typeof(PharmacistShell));
                    break;
                default:
                    ShowError($"Unknown role: {role}. Please contact administrator.");
                    AppSession.Instance.Logout(); // Clear invalid session
                    break;
            }
        }

        private void SetLoadingState(bool isLoading)
        {
            LoginButton.IsEnabled = !isLoading;
            LoginButtonText.Visibility = isLoading ? Visibility.Collapsed : Visibility.Visible;
            LoginLoadingPanel.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
            UsernameBox.IsEnabled = !isLoading;
            PasswordBox.IsEnabled = !isLoading;
            ClinicCodeBox.IsEnabled = !isLoading;
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