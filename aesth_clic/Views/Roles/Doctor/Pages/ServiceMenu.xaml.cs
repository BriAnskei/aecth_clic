using aesth_clic.Tenant.Controller;
using aesth_clic.Tenant.Services;
using aesth_clic.Utils;
using aesth_clic.ViewModels.Doctor;
using aesth_clic.Views.Roles.Doctor.Modals;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

// Alias to avoid collision with the ServiceMenu Page class
using ServiceMenuModel = aesth_clic.Tenant.Model.ServiceMenu;

namespace aesth_clic.Views.Roles.Doctor.Pages
{
    public class ServiceMenuItem : System.ComponentModel.INotifyPropertyChanged
    {
        public int ServiceId { get; set; }

        private string _procedureName = string.Empty;
        public string ProcedureName
        {
            get => _procedureName;
            set { _procedureName = value; OnPropertyChanged(); }
        }

        private string _price = string.Empty;
        public string Price
        {
            get => _price;
            set { _price = value; OnPropertyChanged(); }
        }

        public string AddedByDoctor { get; set; } = string.Empty;
        public string DoctorInitials { get; set; } = string.Empty;
        public decimal RawPrice { get; set; }

        public Microsoft.UI.Xaml.Media.SolidColorBrush DoctorAvatarColor { get; set; } =
            new(Windows.UI.Color.FromArgb(255, 91, 45, 142));

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(
            [System.Runtime.CompilerServices.CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }

    public sealed partial class ServiceMenu : Page
    {
        private readonly MenuController _menuController;
        private readonly ServiceMenuViewModel _vm;

        public ServiceMenu()
        {
            InitializeComponent();

            _menuController = new MenuController(new MenuService());
            _vm = new ServiceMenuViewModel(_menuController);

            ServiceListControl.ItemsSource = _vm.DisplayedServices;

            _vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName
                    is nameof(ServiceMenuViewModel.TotalServices)
                    or nameof(ServiceMenuViewModel.AvgPrice))
                    UpdateKpiCards();
            };

            _ = LoadDataAsync();
        }

        // ──────────────────────────────────────────────────────
        // INITIAL LOAD
        // ──────────────────────────────────────────────────────
        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                await _vm.LoadFromBackendAsync();
                UpdateKpiCards();
            }
            catch (Exception ex)
            {
                ToastHelper.Error(ToastBar, "Failed to load services", ex.Message);
            }
        }

        // ──────────────────────────────────────────────────────
        // KPI CARDS
        // ──────────────────────────────────────────────────────
        private void UpdateKpiCards()
        {
            TxtTotalServices.Text = _vm.TotalServices.ToString();
            TxtAvgPrice.Text = $"₱{_vm.AvgPrice:N0}";
            TxtRowCount.Text =
                $"Showing {_vm.DisplayedServices.Count} " +
                $"service{(_vm.DisplayedServices.Count == 1 ? "" : "s")}";
        }

        // ──────────────────────────────────────────────────────
        // SEARCH
        // ──────────────────────────────────────────────────────
        private void SearchBox_TextChanged(
            AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            _vm.SearchText = sender.Text;
            UpdateKpiCards();
        }

        // ──────────────────────────────────────────────────────
        // ADD SERVICE
        // ──────────────────────────────────────────────────────
        private async void AddServiceButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddEditService(_menuController) { XamlRoot = XamlRoot };
            await dialog.ShowAsync();

            if (dialog.SaveError is not null)
            {
                ToastHelper.Error(ToastBar, "Failed to add service", dialog.SaveError.Message);
                return;
            }

            if (dialog.Result is null) return; // user cancelled

            _vm.AddService(dialog.Result);
            UpdateKpiCards();

            ToastHelper.Success(
                ToastBar,
                "Service added",
                $"{dialog.Result.Name} has been added successfully.");
        }

        // ──────────────────────────────────────────────────────
        // VIEW SERVICE
        // ──────────────────────────────────────────────────────
        private async void ViewService_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
            if (item.DataContext is not ServiceMenuItem service) return;

            var dialog = new ContentDialog
            {
                Title = service.ProcedureName,
                Content = $"Price: {service.Price}\nAdded By: {service.AddedByDoctor}",
                CloseButtonText = "Close",
                XamlRoot = XamlRoot
            };
            await dialog.ShowAsync();
        }

        // ──────────────────────────────────────────────────────
        // EDIT SERVICE
        // ──────────────────────────────────────────────────────
        private async void EditService_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
            if (item.DataContext is not ServiceMenuItem service) return;

            var dialog = new AddEditService(_menuController) { XamlRoot = XamlRoot };
            dialog.LoadForEdit(
                serviceId: service.ServiceId,
                procedureName: service.ProcedureName,
                rawPrice: service.RawPrice);

            await dialog.ShowAsync();

            if (dialog.SaveError is not null)
            {
                ToastHelper.Error(ToastBar, "Failed to update service", dialog.SaveError.Message);
                return;
            }

            if (dialog.Result is null) return; // user cancelled

            ServiceMenuModel r = dialog.Result;
            _vm.UpdateService(r.Id, r.Name, (decimal)r.Price);
            UpdateKpiCards();

            ToastHelper.Success(
                ToastBar,
                "Service updated",
                $"{r.Name} has been updated successfully.");
        }

        // ──────────────────────────────────────────────────────
        // DELETE SERVICE
        // ──────────────────────────────────────────────────────
        private async void DeleteService_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
            if (item.DataContext is not ServiceMenuItem service) return;

            var dialog = new DeleteService(_menuController) { XamlRoot = XamlRoot };
            dialog.LoadService(service.ServiceId, service.ProcedureName);

            await dialog.ShowAsync();

            if (dialog.SaveError is not null)
            {
                ToastHelper.Error(ToastBar, "Failed to delete service", dialog.SaveError.Message);
                return;
            }

            if (!dialog.Deleted) return; // user cancelled

            _vm.DeleteService(service.ServiceId);
            UpdateKpiCards();

            ToastHelper.Success(
                ToastBar,
                "Service deleted",
                $"{service.ProcedureName} has been permanently removed.");
        }
    }
}