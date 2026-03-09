using aesth_clic.Tenant.Controller;
using aesth_clic.Utils;
using aesth_clic.ViewModels.Admin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace aesth_clic.Views.Roles.Admin.Pages
{
    public sealed partial class AdminDashboard : Page
    {
        private readonly AdminDashboardViewModel _vm = new();
        private readonly MasterDashboardController _dashboardController;

        public AdminDashboard()
        {
            InitializeComponent();
            _dashboardController = App.Services.GetRequiredService<MasterDashboardController>();
            ProcedureListControl.ItemsSource = _vm.TopProcedures;
            LowStockListControl.ItemsSource = _vm.LowStockMedicines;
            _ = LoadFromDbAsync();
        }

        // ── Data loading ──────────────────────────────────────────────────────
        private async System.Threading.Tasks.Task LoadFromDbAsync()
        {
            try
            {
                var dto = await _dashboardController.GetDashboardDataAsync();
                _vm.LoadFromDto(dto);

                TxtTotalPatients.Text = _vm.TotalPatients;
                TxtMonthlyRevenue.Text = _vm.MonthlyRevenue;
                TxtLowStockKpi.Text = _vm.LowStockCount.ToString();

                TxtLowStockCount.Text = _vm.LowStockCountDisplay;
                TxtLowStockAlert.Text = _vm.LowStockCount > 0
                    ? $"{_vm.LowStockCount} item{(_vm.LowStockCount != 1 ? "s" : "")} need restocking. Please notify the pharmacist."
                    : "All medicine stock levels are healthy.";

                LowStockAlertPanel.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                ToastHelper.Error(ToastBar, "Failed to load dashboard", ex.Message);
            }
        }

        // ── KPI Card click → navigate ContentFrame to User Management ────────
        private void KpiCard_Click(object sender, RoutedEventArgs e)
        {
            // 'this.Frame' is the ContentFrame inside AdminClientShell.
            // Navigating it directly mirrors what the sidebar nav does.
            Frame?.Navigate(typeof(UserManagement));

            // Sync the NavigationView sidebar highlight to "Users Management"
            // Walk up: Frame → NavigationView content area → NavigationView
            if (Frame?.Parent is Microsoft.UI.Xaml.Controls.NavigationView navView)
            {
                foreach (var item in navView.MenuItems)
                {
                    if (item is NavigationViewItem nvi && nvi.Tag?.ToString() == "UsersManagement")
                    {
                        navView.SelectedItem = nvi;
                        break;
                    }
                }
            }
        }
    }
}