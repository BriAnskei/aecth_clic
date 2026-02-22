using System.Runtime.Versioning;  // ← add this
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using aesth_clic.Views;

namespace aesth_clic
{
    public sealed partial class MainWindow : Window
    {
        [SupportedOSPlatform("windows10.0.17763.0")]  // ← add this
        public MainWindow()
        {
            InitializeComponent();
            RootFrame.Navigate(typeof(RoleSelectionPage));
            //RootFrame.Navigate(typeof(LoginPage));
        }
    }
}