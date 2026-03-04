using aesth_clic.Tenant.Controller;
using aesth_clic.Utils;
using aesth_clic.ViewModels.Admin;
using aesth_clic.Views.Roles.Admin.Modals;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;

namespace aesth_clic.Views.Roles.Admin.Pages
{
    public class StaffUserItem : System.ComponentModel.INotifyPropertyChanged
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;

        public Microsoft.UI.Xaml.Media.SolidColorBrush AvatarColor { get; set; } = new(Windows.UI.Color.FromArgb(255, 91, 45, 142));
        public Microsoft.UI.Xaml.Media.SolidColorBrush RoleBadgeColor { get; set; } = new(Microsoft.UI.Colors.Gray);

        private string _status = "Active";
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusBadgeColor));
                OnPropertyChanged(nameof(DeactivateVisible));
                OnPropertyChanged(nameof(ReactivateVisible));
                OnPropertyChanged(nameof(DeleteVisible));
            }
        }

        public Microsoft.UI.Xaml.Media.SolidColorBrush StatusBadgeColor =>
            Status == "Active"
                ? new(Windows.UI.Color.FromArgb(255, 14, 164, 122))
                : new(Windows.UI.Color.FromArgb(255, 192, 57, 43));

        public Microsoft.UI.Xaml.Visibility DeactivateVisible =>
            Status == "Active" ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;

        public Microsoft.UI.Xaml.Visibility ReactivateVisible =>
            Status == "Deactivated" ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;

        public Microsoft.UI.Xaml.Visibility DeleteVisible =>
            Status == "Deactivated" ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(
            [System.Runtime.CompilerServices.CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }
}

// ─────────────────────────────────────────────────────────────
// PAGE CODE-BEHIND
// ─────────────────────────────────────────────────────────────

namespace aesth_clic.Views.Roles.Admin.Pages
{
    public sealed partial class UserManagement : Page
    {
        private readonly UserManagementViewModel _vm = new();
        private readonly UserController _userController;

        public UserManagement()
        {
            InitializeComponent();

            _userController = App.Services.GetRequiredService<UserController>();

            UserListControl.ItemsSource = _vm.DisplayedUsers;

            _vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName
                    is nameof(UserManagementViewModel.TotalUsers)
                    or nameof(UserManagementViewModel.ActiveUsers)
                    or nameof(UserManagementViewModel.DeactivatedUsers))
                    UpdateKpiCards();

                if (e.PropertyName == nameof(UserManagementViewModel.IsLoading))
                    UpdateLoadingState(_vm.IsLoading);
            };

            _ = LoadFromDbAsync();
        }

        // ──────────────────────────────────────────────────────
        // LOADING STATE
        // ──────────────────────────────────────────────────────
        private void UpdateLoadingState(bool isLoading)
        {
            KpiGrid.IsHitTestVisible = !isLoading;
            KpiGrid.Opacity = isLoading ? 0.4 : 1.0;
            FilterToolbar.IsHitTestVisible = !isLoading;
            FilterToolbar.Opacity = isLoading ? 0.4 : 1.0;

            SkeletonTable.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
            RealTable.Visibility = isLoading ? Visibility.Collapsed : Visibility.Visible;
        }

        // ──────────────────────────────────────────────────────
        // DATA LOADING
        // ──────────────────────────────────────────────────────
        private async System.Threading.Tasks.Task LoadFromDbAsync()
        {
            _vm.IsLoading = true;
            try
            {
                var users = await _userController.GetAllUsersAsync();

                _vm.LoadFromDb(users.Select(u => (
                    Id: u.Id.ToString(),
                    Name: u.FullName,
                    Email: u.Email,
                    Phone: u.PhoneNumber,
                    Role: u.Role,
                    Status: u.AccountStatus?.Status ?? "active",
                    Username: u.Username
                )));

                // Reassign ItemsSource to force ItemsControl to fully re-render
                // all rows. x:Bind is one-time by default, so without this the
                // table won't reflect updated Status, badge colours, etc.
                UserListControl.ItemsSource = null;
                UserListControl.ItemsSource = _vm.DisplayedUsers;

                UpdateKpiCards();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoadFromDbAsync] Exception: {ex.Message}");
                ToastHelper.Error(ToastBar, "Failed to load users", ex.Message);
            }
            finally
            {
                _vm.IsLoading = false;
            }
        }

        // ──────────────────────────────────────────────────────
        // KPI CARDS
        // ──────────────────────────────────────────────────────
        private void UpdateKpiCards()
        {
            if (TxtTotalUsers is null || TxtActiveUsers is null ||
                TxtInactiveUsers is null || TxtRowCount is null)
                return;

            TxtTotalUsers.Text = _vm.TotalUsers.ToString();
            TxtActiveUsers.Text = _vm.ActiveUsers.ToString();
            TxtInactiveUsers.Text = _vm.DeactivatedUsers.ToString();
            TxtRowCount.Text =
                $"Showing {_vm.DisplayedUsers.Count} of {_vm.TotalUsers} " +
                $"user{(_vm.TotalUsers != 1 ? "s" : "")}";
        }

        // ──────────────────────────────────────────────────────
        // SEARCH + FILTERS
        // ──────────────────────────────────────────────────────
        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            _vm.SearchText = sender.Text;
            UpdateKpiCards();
        }

        private void RoleFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _vm.SelectedRole =
                (RoleFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All";
            UpdateKpiCards();
        }

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _vm.SelectedStatus =
                (StatusFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All";
            UpdateKpiCards();
        }

        // ──────────────────────────────────────────────────────
        // ADD USER
        // ──────────────────────────────────────────────────────
        private async void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddNewUser(_userController) { XamlRoot = XamlRoot };
            await dialog.ShowAsync();

            // User cancelled
            if (dialog.Result is null && dialog.SaveError is null) return;

            // Controller threw — show error toast
            if (dialog.SaveError is not null)
            {
                ToastHelper.Error(ToastBar, "Failed to add user", dialog.SaveError.Message);
                return;
            }

            // Success — refresh from DB then show toast
            await LoadFromDbAsync();
            ToastHelper.Success(ToastBar, "User added",
                $"{dialog.Result!.FullName} has been added as {dialog.Result.Role}.");
        }

        // ──────────────────────────────────────────────────────
        // EDIT USER
        // ──────────────────────────────────────────────────────
        private async void EditUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
            if (item.DataContext is not StaffUserItem user) return;

            var dialog = new AddNewUser(_userController) { XamlRoot = XamlRoot };
            dialog.LoadForEdit(int.Parse(user.UserId), user.FullName, user.Email, user.Phone, user.Role, user.Username);
            await dialog.ShowAsync();

            // User cancelled
            if (dialog.Result is null && dialog.SaveError is null) return;

            // Controller threw — show error toast
            if (dialog.SaveError is not null)
            {
                ToastHelper.Error(ToastBar, "Failed to update user", dialog.SaveError.Message);
                return;
            }

            // Success — refresh from DB then show toast
            await LoadFromDbAsync();
            ToastHelper.Success(ToastBar, "User updated",
                $"{dialog.Result!.FullName} has been updated.");
        }

        // ──────────────────────────────────────────────────────
        // DEACTIVATE
        // ──────────────────────────────────────────────────────
        private async void DeactivateUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
            if (item.DataContext is not StaffUserItem user) return;

            var dlg = new DeactivateUser(user, _userController) { XamlRoot = XamlRoot };
            await dlg.ShowAsync();

            // User cancelled — admin closed dialog without confirming
            if (!dlg.Confirmed) return;

            // Controller threw — show error toast
            if (dlg.SaveError is not null)
            {
                ToastHelper.Error(ToastBar, "Failed to deactivate user", dlg.SaveError.Message);
                return;
            }

            // Success — refresh from DB then show toast
            await LoadFromDbAsync();
            ToastHelper.Warning(ToastBar, "User deactivated",
                $"{user.FullName} has been deactivated.");
        }

        // ──────────────────────────────────────────────────────
        // REACTIVATE
        // ──────────────────────────────────────────────────────
        private async void ReactivateUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
            if (item.DataContext is not StaffUserItem user) return;

            var dlg = new ReactivateUser(user, _userController) { XamlRoot = XamlRoot };
            await dlg.ShowAsync();

            // User cancelled — admin closed dialog without confirming
            if (!dlg.Confirmed) return;

            // Controller threw — show error toast
            if (dlg.SaveError is not null)
            {
                ToastHelper.Error(ToastBar, "Failed to reactivate user", dlg.SaveError.Message);
                return;
            }

            // Success — refresh from DB then show toast
            await LoadFromDbAsync();
            ToastHelper.Success(ToastBar, "User reactivated",
                $"{user.FullName} has been reactivated.");
        }

        // ──────────────────────────────────────────────────────
        // DELETE
        // ──────────────────────────────────────────────────────
        private async void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
            if (item.DataContext is not StaffUserItem user) return;
            if (user.Status != "Deactivated") return;

            var dlg = new DeleteUser(user, _userController) { XamlRoot = XamlRoot };
            await dlg.ShowAsync();

            // User cancelled
            if (!dlg.Confirmed) return;

            // Controller threw — show error toast
            if (dlg.SaveError is not null)
            {
                ToastHelper.Error(ToastBar, "Failed to delete user", dlg.SaveError.Message);
                return;
            }

            // Success — refresh from DB then show toast
            await LoadFromDbAsync();
            ToastHelper.Success(ToastBar, "User deleted",
                $"{user.FullName} has been permanently deleted.");
        }
    }
}