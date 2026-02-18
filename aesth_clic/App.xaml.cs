using System.Runtime.Versioning;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel.Activation;

namespace aesth_clic
{
    public partial class App : Application
    {
        private Window? _window;
        public Window? MainWindow => _window;  // ← expose it

        public App()
        {
            InitializeComponent();
        }

        [SupportedOSPlatform("windows10.0.17763.0")]
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            _window.Activate();
        }
    }
}