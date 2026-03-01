using aesth_clic.Utils;
using aesth_clic.ViewModels.SuperAdmin;
using aesth_clic.Views.Roles.SuperAdmin.Modals;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using Windows.UI;

namespace aesth_clic.Views.Roles.SuperAdmin.Pages
{
    public class UserItem : System.ComponentModel.INotifyPropertyChanged
    {
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public int RowNumber { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string ClinicName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;

        private string _status = "Active";
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusBadgeColor));
                OnPropertyChanged(nameof(StatusBadgeText));
                OnPropertyChanged(nameof(DeactivateVisible));
                OnPropertyChanged(nameof(ReactivateVisible));
                OnPropertyChanged(nameof(DeleteVisible));
            }
        }

        private string _tier = "basic";
        public string Tier
        {
            get => _tier;
            set
            {
                _tier = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TierDisplay));
                OnPropertyChanged(nameof(TierBadgeText));
            }
        }

        public string Initials
        {
            get
            {
                var parts = FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                    return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
                return FullName.Length > 0 ? FullName[0].ToString().ToUpper() : "?";
            }
        }

        public Microsoft.UI.Xaml.Media.SolidColorBrush AvatarColor =>
            new(Color.FromArgb(255, 0, 120, 212));

        public Microsoft.UI.Xaml.Media.SolidColorBrush StatusBadgeColor =>
            Status == "Active"
                ? new(Color.FromArgb(20, 14, 164, 122))
                : new(Color.FromArgb(20, 216, 59, 1));

        public Microsoft.UI.Xaml.Media.SolidColorBrush StatusBadgeText =>
            Status == "Active"
                ? new(Color.FromArgb(255, 10, 130, 96))
                : new(Color.FromArgb(255, 180, 40, 0));

        public Microsoft.UI.Xaml.Visibility DeactivateVisible =>
            Status == "Active"
                ? Microsoft.UI.Xaml.Visibility.Visible
                : Microsoft.UI.Xaml.Visibility.Collapsed;

        public Microsoft.UI.Xaml.Visibility ReactivateVisible =>
            Status == "Deactivated"
                ? Microsoft.UI.Xaml.Visibility.Visible
                : Microsoft.UI.Xaml.Visibility.Collapsed;

        public Microsoft.UI.Xaml.Visibility DeleteVisible =>
            Status == "Deactivated"
                ? Microsoft.UI.Xaml.Visibility.Visible
                : Microsoft.UI.Xaml.Visibility.Collapsed;

        public string TierDisplay =>
            Tier switch
            {
                "basic" => "Basic",
                "standard" => "Standard",
                "premium" => "Premium",
                _ => Tier,
            };

        public Microsoft.UI.Xaml.Media.SolidColorBrush TierBadgeText =>
            Tier switch
            {
                "basic" => new(Color.FromArgb(255, 135, 100, 184)),
                "standard" => new(Color.FromArgb(255, 193, 156, 0)),
                "premium" => new(Color.FromArgb(255, 0, 153, 188)),
                _ => new(Color.FromArgb(255, 100, 100, 100)),
            };

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(
            [System.Runtime.CompilerServices.CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }
}

// ─────────────────────────────────────────────────────────────
// PAGE CODE-BEHIND
// ─────────────────────────────────────────────────────────────

namespace aesth_clic.Views.Roles.SuperAdmin.Pages
{
    public sealed partial class UserManagement : Page
    {
        private readonly UserManagementViewModel _vm = new();
        private readonly aesth_clic.Master.Controller.CompanyController _companyController;

        public UserManagement()
        {
            InitializeComponent();

            _companyController = App.Services
                .GetRequiredService<aesth_clic.Master.Controller.CompanyController>();

            UserListControl.ItemsSource = _vm.DisplayedUsers;

            _vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName
                    is nameof(UserManagementViewModel.TotalUsers)
                    or nameof(UserManagementViewModel.ActiveUsers)
                    or nameof(UserManagementViewModel.DeactivatedUsers))
                    UpdateKpiCards();
            };

            _ = LoadFromDbAsync();
        }

        // ──────────────────────────────────────────────────────
        // DATA LOADING  (mock until GetAllClientsAsync is ready)
        // ──────────────────────────────────────────────────────
        private async System.Threading.Tasks.Task LoadFromDbAsync()
        {
            try
            {
                // TODO: swap mock list for real call once GetAllClientsAsync is implemented:
                //   var clients = await _companyController.GetAllClientsAsync();
                //   _vm.LoadFromDb(clients);

                var mockClients = new List<(
                    aesth_clic.Master.Model.Client Client,
                    string FullName,
                    string Email,
                    string Phone,
                    string Username)>
                {
                    (
                        new aesth_clic.Master.Model.Client
                        {
                            Id = 1, ClinicName = "Santos Aesthetic Clinic",
                            DbName = "Aesth_Santos_Aesthetic_Clinic",
                            ClinicCode = "12345", Status = "active",
                            Tier = "basic", CreatedAt = DateTime.UtcNow
                        },
                        "Maria Santos", "maria@santos.com", "09171234567", "santos_clinic1234"
                    ),
                    (
                        new aesth_clic.Master.Model.Client
                        {
                            Id = 2, ClinicName = "Glow Skin Center",
                            DbName = "Aesth_Glow_Skin_Center",
                            ClinicCode = "67890", Status = "active",
                            Tier = "premium", CreatedAt = DateTime.UtcNow
                        },
                        "Jose Reyes", "jose@glow.com", "09281234567", "glow_clinic5678"
                    ),
                    (
                        new aesth_clic.Master.Model.Client
                        {
                            Id = 3, ClinicName = "Lumina Derma Clinic",
                            DbName = "Aesth_Lumina_Derma_Clinic",
                            ClinicCode = "11223", Status = "deactivated",
                            Tier = "standard", CreatedAt = DateTime.UtcNow
                        },
                        "Ana Dela Cruz", "ana@lumina.com", "09391234567", "lumina_clinic9012"
                    ),
                };

                _vm.LoadFromMock(mockClients);
                UpdateKpiCards();
            }
            catch (Exception ex)
            {
                ToastHelper.Error(ToastBar, "Failed to load clients", ex.Message);
            }
        }

        // ──────────────────────────────────────────────────────
        // KPI CARDS
        // ──────────────────────────────────────────────────────
        private void UpdateKpiCards()
        {
            TxtTotalUsers.Text = _vm.TotalUsers.ToString();
            TxtActiveUsers.Text = _vm.ActiveUsers.ToString();
            TxtInactiveUsers.Text = _vm.DeactivatedUsers.ToString();

            TxtRowCount.Text =
                $"Showing {_vm.DisplayedUsers.Count} of {_vm.TotalUsers} " +
                $"client{(_vm.TotalUsers != 1 ? "s" : "")}";
        }

        // ──────────────────────────────────────────────────────
        // SEARCH + FILTERS
        // ──────────────────────────────────────────────────────
        private void SearchBox_TextChanged(
            AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            _vm.SearchText = sender.Text;
            UpdateKpiCards();
        }

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _vm.SelectedStatus =
                (StatusFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All";
            UpdateKpiCards();
        }

        private void TierFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _vm.SelectedTier =
                (TierFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All";
            UpdateKpiCards();
        }

        // ──────────────────────────────────────────────────────
        // ADD CLIENT
        // ──────────────────────────────────────────────────────
        private void KebabMenu_Click(object sender, RoutedEventArgs e) { }

        private async void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddNewClient { XamlRoot = XamlRoot };
            await dialog.ShowAsync();

            // Result is only non-null when the dialog saved successfully
            if (dialog.Result is null)
                return;

            var r = dialog.Result;

            await LoadFromDbAsync();

            ToastHelper.Success(
                ToastBar,
                "Client added",
                $"{r.FullName} ({r.ClinicName}) has been created successfully.");
        }


        // ──────────────────────────────────────────────────────
        // EDIT CLIENT  — TODO: wire to new controller when ready
        // ──────────────────────────────────────────────────────
        private async void EditUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
            int userId = (int)item.Tag;

            var user = _vm.FindUser(userId);
            if (user == null) return;

            var dialog = new EditClient(user) { XamlRoot = XamlRoot };
            var dialogResult = await dialog.ShowAsync();

            if (dialogResult == ContentDialogResult.Primary && dialog.Result is not null)
            {
                // TODO: replace stub with real controller call once UpdateClientAsync is implemented
                //   await _companyController.UpdateClientAsync(...)
                ToastHelper.Error(ToastBar, "Not implemented", "Edit client is not yet available.");
            }
        }

        // ──────────────────────────────────────────────────────
        // MANAGE MODULES  — TODO: wire to new controller when ready
        // ──────────────────────────────────────────────────────
        private async void ManageModules_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
            int userId = (int)item.Tag;

            var user = _vm.FindUser(userId);
            if (user == null) return;

            var dialog = new ManageModules(user) { XamlRoot = XamlRoot };
            await dialog.ShowAsync();

            if (dialog.Result is not null)
            {
                // TODO: replace stub with real controller call once UpdateTierAsync is implemented
                //   await _companyController.UpdateTierAsync(user.CompanyId, newTier)
                ToastHelper.Error(ToastBar, "Not implemented", "Manage modules is not yet available.");
            }
        }

        // ──────────────────────────────────────────────────────
        // DEACTIVATE  — TODO: wire to new controller when ready
        // ──────────────────────────────────────────────────────
        private async void DeactivateUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
            int userId = (int)item.Tag;

            var user = _vm.FindUser(userId);
            if (user == null) return;

            var dialog = new DeactivateClient(user) { XamlRoot = XamlRoot };
            await dialog.ShowAsync();

            if (dialog.Confirmed)
            {
                // TODO: replace stub with real controller call once DeactivateClientAsync is implemented
                //   await _companyController.DeactivateClientAsync(user.CompanyId)
                //   _vm.DeactivateUser(userId);
                ToastHelper.Error(ToastBar, "Not implemented", "Deactivate client is not yet available.");
            }
        }

        // ──────────────────────────────────────────────────────
        // REACTIVATE  — TODO: wire to new controller when ready
        // ──────────────────────────────────────────────────────
        private async void ReactivateUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
            int userId = (int)item.Tag;

            var user = _vm.FindUser(userId);
            if (user == null) return;

            var dialog = new ReactivateClient(user) { XamlRoot = XamlRoot };
            await dialog.ShowAsync();

            if (dialog.Confirmed)
            {
                // TODO: replace stub with real controller call once ReactivateClientAsync is implemented
                //   await _companyController.ReactivateClientAsync(user.CompanyId)
                //   _vm.ReactivateUser(userId);
                ToastHelper.Error(ToastBar, "Not implemented", "Reactivate client is not yet available.");
            }
        }

        // ──────────────────────────────────────────────────────
        // DELETE  — TODO: wire to new controller when ready
        // ──────────────────────────────────────────────────────
        private async void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
            int userId = (int)item.Tag;

            var user = _vm.FindUser(userId);
            if (user == null || user.Status != "Deactivated") return;

            var dialog = new DeleteClient(user) { XamlRoot = XamlRoot };
            await dialog.ShowAsync();

            if (dialog.Confirmed)
            {
                // TODO: replace stub with real controller call once DeleteClientAsync is implemented
                //   await _companyController.DeleteClientAsync(user.UserId)
                //   _vm.DeleteUser(userId);
                ToastHelper.Error(ToastBar, "Not implemented", "Delete client is not yet available.");
            }
        }
    }
}