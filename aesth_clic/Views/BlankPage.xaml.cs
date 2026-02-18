using System.Runtime.Versioning;  // ← add this
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace aesth_clic.Views
{
    public sealed partial class BlankPage : Page
    {
        public BlankPage()
        {
            InitializeComponent();
        }

        [SupportedOSPlatform("windows10.0.17763.0")]  // ← add this
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is string sectionName)
            {
                SectionLabel.Text = sectionName;
            }
        }
    }
}