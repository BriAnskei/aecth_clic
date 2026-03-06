using aesth_clic.Views;
using Microsoft.UI.Xaml;
using System.Runtime.Versioning;  // ← add this

namespace aesth_clic
{
    public sealed partial class MainWindow : Window
    {
        [SupportedOSPlatform("windows10.0.17763.0")]  // ← add this
        public MainWindow()
        {
            InitializeComponent();
            //RootFrame.Navigate(typeof(RoleSelectionPage));
            RootFrame.Navigate(typeof(LoginPage));
        }
    }
}