using System;
using aesth_clic.Controller;
using aesth_clic.Models.Companies;
using aesth_clic.Models.Users;
using aesth_clic.Utils;
using aesth_clic.ViewModels.SuperAdmin;
using aesth_clic.Views.Roles.SuperAdmin.Modals;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
                var parts = FullName.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
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
            [System.Runtime.CompilerServices.CallerMemberName] string? name = null
        ) =>
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
        private readonly UserController _userController;
        private readonly CompanyController _companyController;

        public UserManagement()
        {
            InitializeComponent();
            _userController = App.Services.GetRequiredService<UserController>();
            _companyController = App.Services.GetRequiredService<CompanyController>();

            UserListControl.ItemsSource = _vm.DisplayedUsers;

            _vm.PropertyChanged += (_, e) =>
            {
                if (
                    e.PropertyName
                    is nameof(UserManagementViewModel.TotalUsers)
                        or nameof(UserManagementViewModel.ActiveUsers)
                        or nameof(UserManagementViewModel.DeactivatedUsers)
                )
                    UpdateKpiCards();
            };

            _ = LoadFromDbAsync();
        }

        private async System.Threading.Tasks.Task LoadFromDbAsync()
        {
            try
            {
                var clients = await _userController.GetAllAdminsAsync();
                _vm.LoadFromDb(clients);
                UpdateKpiCards();
            }
            catch (Exception ex)
            {
                ToastHelper.Error(ToastBar, "Failed to load clients", ex.Message);
            }
        }

        private void UpdateKpiCards()
        {
            TxtTotalUsers.Text = _vm.TotalUsers.ToString();
            TxtActiveUsers.Text = _vm.ActiveUsers.ToString();
            TxtInactiveUsers.Text = _vm.DeactivatedUsers.ToString();

            TxtRowCount.Text =
                $"Showing {_vm.DisplayedUsers.Count} of {_vm.TotalUsers} client{(_vm.TotalUsers != 1 ? "s" : "")}";
        }

        private void SearchBox_TextChanged(
            AutoSuggestBox sender,
            AutoSuggestBoxTextChangedEventArgs args
        )
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
            _vm.SelectedTier = (TierFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All";
            UpdateKpiCards();
        }

        private void KebabMenu_Click(object sender, RoutedEventArgs e) { }

        private async void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddNewClient { XamlRoot = XamlRoot };
            var result = await dialog.ShowAsync();

            if (result != ContentDialogResult.Primary || dialog.Result is null)
                return;

            var r = dialog.Result;

            var adminClient = new AdminClients(
                new User(
                    id: 0,
                    fullName: r.FullName,
                    email: r.Email,
                    username: r.Username,
                    password: r.Password,
                    phoneNumber: r.PhoneNumber,
                    role: "admin",
                    createdAt: DateTime.Now
                ),
                new Company(
                    id: 0,
                    owner_id: 0,
                    name: r.ClinicName,
                    status: "active",
                    module_tier: r.ModuleTier
                )
            );

            try
            {
                await _userController.CreateAdminClientAsync(adminClient);
                await LoadFromDbAsync();
                ToastHelper.Success(
                    ToastBar,
                    "Client added",
                    $"{r.FullName} ({r.ClinicName}) has been created successfully."
                );
            }
            catch (Exception ex)
            {
                ToastHelper.Error(ToastBar, "Failed to create client", ex.Message);
            }
        }

        private async void EditUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item)
                return;
            int userId = (int)item.Tag;

            var user = _vm.FindUser(userId);
            if (user == null)
                return;

            var dialog = new EditClient(user) { XamlRoot = XamlRoot };
            var dialogResult = await dialog.ShowAsync();

            if (dialogResult == ContentDialogResult.Primary && dialog.Result is not null)
            {
                var r = dialog.Result;

                var adminClient = new AdminClients(
                    new User(
                        id: user.UserId,
                        fullName: r.FullName,
                        email: r.Email,
                        username: r.Username,
                        password: r.Password ?? string.Empty,
                        phoneNumber: r.PhoneNumber,
                        role: "admin",
                        createdAt: DateTime.Now
                    ),
                    new Company(
                        id: user.CompanyId,
                        owner_id: user.UserId,
                        name: r.ClinicName,
                        status: user.Status == "Active" ? "active" : "deactivated",
                        module_tier: user.Tier
                    )
                );

                try
                {
                    await _userController.UpdateAdminClientAsync(adminClient);

                    // Update local state
                    user.FullName = r.FullName;
                    user.Email = r.Email;
                    user.Phone = r.PhoneNumber;
                    user.ClinicName = r.ClinicName;
                    user.Username = r.Username;
                    if (r.Password is not null)
                        user.Password = r.Password;

                    _vm.ApplyFilters();
                    UpdateKpiCards();
                    ToastHelper.Success(
                        ToastBar,
                        "Client updated",
                        $"{r.FullName}'s details have been saved successfully."
                    );
                }
                catch (Exception ex)
                {
                    ToastHelper.Error(ToastBar, "Failed to update client", ex.Message);
                }
            }
        }

        // ← UPDATED
        private async void ManageModules_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item)
                return;
            int userId = (int)item.Tag;

            var user = _vm.FindUser(userId);
            if (user == null)
                return;

            var dialog = new ManageModules(user) { XamlRoot = XamlRoot };
            await dialog.ShowAsync();

            if (dialog.Result is not null)
            {
                string newTier = dialog.Result.Tier.ToLower(); // "Basic" → "basic"

                try
                {
                    await _userController.UpdateAdminTierAsync(user.CompanyId, newTier);
                    _vm.UpdateUserTier(userId, newTier);
                    UpdateKpiCards();
                    ToastHelper.Success(
                        ToastBar,
                        "Tier updated",
                        $"{user.FullName}'s tier has been changed to {dialog.Result.Tier}."
                    );
                }
                catch (Exception ex)
                {
                    ToastHelper.Error(ToastBar, "Failed to update tier", ex.Message);
                }
            }
        }

        private async void DeactivateUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item)
                return;
            int userId = (int)item.Tag;

            var user = _vm.FindUser(userId);
            if (user == null)
                return;

            var dialog = new DeactivateClient(user) { XamlRoot = XamlRoot };
            await dialog.ShowAsync();

            if (dialog.Confirmed)
            {
                try
                {
                    _vm.DeactivateUser(userId);
                    await _companyController.UpdateCompanyStatusAsync(user.UserId, "deactivated");
                    UpdateKpiCards();
                    ToastHelper.Success(
                        ToastBar,
                        "Client deactivated",
                        $"{user.FullName} has been deactivated."
                    );
                }
                catch (Exception ex)
                {
                    ToastHelper.Error(ToastBar, "Failed to deactivate client", ex.Message);
                }
            }
        }

        private async void ReactivateUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item)
                return;
            int userId = (int)item.Tag;

            var user = _vm.FindUser(userId);
            if (user == null)
                return;

            var dialog = new ReactivateClient(user) { XamlRoot = XamlRoot };
            await dialog.ShowAsync();

            if (dialog.Confirmed)
            {
                try
                {
                    _vm.ReactivateUser(userId);
                    await _companyController.UpdateCompanyStatusAsync(user.UserId, "active");
                    UpdateKpiCards();
                    ToastHelper.Success(
                        ToastBar,
                        "Client reactivated",
                        $"{user.FullName} has been reactivated."
                    );
                }
                catch (Exception ex)
                {
                    ToastHelper.Error(ToastBar, "Failed to reactivate client", ex.Message);
                }
            }
        }

        private async void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item)
                return;
            int userId = (int)item.Tag;

            var user = _vm.FindUser(userId);
            if (user == null || user.Status != "Deactivated")
                return;

            var dialog = new DeleteClient(user) { XamlRoot = XamlRoot };
            await dialog.ShowAsync();

            if (dialog.Confirmed)
            {
                try
                {
                    await _userController.DeleteAdminClientAsync(user.UserId);
                    _vm.DeleteUser(userId);
                    UpdateKpiCards();
                    ToastHelper.Success(
                        ToastBar,
                        "Client deleted",
                        $"{user.FullName} has been permanently deleted."
                    );
                }
                catch (Exception ex)
                {
                    ToastHelper.Error(ToastBar, "Failed to delete client", ex.Message);
                }
            }
        }
    }
}
