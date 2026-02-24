using aesth_clic.Models.Users;
using aesth_clic.Views.Roles.SuperAdmin.Pages;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.UI;
using Microsoft.UI.Xaml.Media;

namespace aesth_clic.Views.Roles.SuperAdmin.Pages
{
    // ─────────────────────────────────────────────────────────
    // USER ITEM MODEL
    // ─────────────────────────────────────────────────────────
    public class UserItem : System.ComponentModel.INotifyPropertyChanged
    {
        public int UserId { get; set; }
        public int RowNumber { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string ClinicName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // ── Status ───────────────────────────────────────────

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

        // ── Tier ─────────────────────────────────────────────

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

        // ── Derived display helpers — Status ─────────────────

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

        public Microsoft.UI.Xaml.Media.SolidColorBrush StatusBadgeColor => Status == "Active"
            ? new(Color.FromArgb(20, 14, 164, 122))
            : new(Color.FromArgb(20, 216, 59, 1));

        public Microsoft.UI.Xaml.Media.SolidColorBrush StatusBadgeText => Status == "Active"
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

        // ── Derived display helpers — Tier ────────────────────

        /// <summary>Capitalised display label for the TIER column.</summary>
        public string TierDisplay => Tier switch
        {
            "basic" => "Basic",
            "standard" => "Standard",
            "premium" => "Premium",
            _ => Tier
        };

        /// <summary>
        /// Colored brush for the tier label.
        /// Basic   = #8764B8 (purple)   — distinct from Active (#0EA47A) and Deactivated (#D83B01)
        /// Standard= #C19C00 (gold)
        /// Premium = #0099BC (cyan-teal)
        /// </summary>
        public Microsoft.UI.Xaml.Media.SolidColorBrush TierBadgeText => Tier switch
        {
            "basic" => new(Color.FromArgb(255, 135, 100, 184)),
            "standard" => new(Color.FromArgb(255, 193, 156, 0)),
            "premium" => new(Color.FromArgb(255, 0, 153, 188)),
            _ => new(Color.FromArgb(255, 100, 100, 100))
        };

        // ── INotifyPropertyChanged ────────────────────────────

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }
}

// ─────────────────────────────────────────────────────────────
// PAGE CODE-BEHIND
// ─────────────────────────────────────────────────────────────

namespace aesth_clic.Views.Roles.SuperAdmin.Pages
{
    using aesth_clic.Controller;
    using aesth_clic.Models.Companies;
    using aesth_clic.Models.Users;
    using aesth_clic.Utils;
    using aesth_clic.ViewModels.SuperAdmin;
    using aesth_clic.Views.Roles.SuperAdmin.Modals;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using System;

    public sealed partial class UserManagement : Page
    {
        private readonly UserManagementViewModel _vm = new();
        private readonly UserController _userController;

        public UserManagement()
        {
            InitializeComponent();
            _userController = App.Services.GetRequiredService<UserController>();

            // Bind ItemsControl to the ViewModel's DisplayedUsers collection
            UserListControl.ItemsSource = _vm.DisplayedUsers;

            // Sync KPI cards whenever ViewModel KPI props change
            _vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is nameof(UserManagementViewModel.TotalUsers)
                                   or nameof(UserManagementViewModel.ActiveUsers)
                                   or nameof(UserManagementViewModel.DeactivatedUsers))
                    UpdateKpiCards();
            };

            // Load real data from DB on startup
            _ = LoadFromDbAsync();
        }

        // ─────────────────────────────────────────
        // DB LOAD
        // ─────────────────────────────────────────
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

        // ─────────────────────────────────────────
        // KPI CARDS
        // ─────────────────────────────────────────
        private void UpdateKpiCards()
        {
            TxtTotalUsers.Text = _vm.TotalUsers.ToString();
            TxtActiveUsers.Text = _vm.ActiveUsers.ToString();
            TxtInactiveUsers.Text = _vm.DeactivatedUsers.ToString();

            TxtRowCount.Text =
                $"Showing {_vm.DisplayedUsers.Count} of {_vm.TotalUsers} client{(_vm.TotalUsers != 1 ? "s" : "")}";
        }

        // ─────────────────────────────────────────
        // TOOLBAR EVENTS
        // ─────────────────────────────────────────
        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
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

        // ─────────────────────────────────────────
        // KEBAB MENU (no-op; flyout handles it)
        // ─────────────────────────────────────────
        private void KebabMenu_Click(object sender, RoutedEventArgs e) { }

        // ─────────────────────────────────────────
        // ADD CLIENT
        // ─────────────────────────────────────────
        private async void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddNewClient { XamlRoot = XamlRoot };
            var result = await dialog.ShowAsync();

            if (result != ContentDialogResult.Primary || dialog.Result is null) return;

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
                    $"{r.FullName} ({r.ClinicName}) has been created successfully.");
            }
            catch (Exception ex)
            {
                ToastHelper.Error(ToastBar, "Failed to create client", ex.Message);
            }
        }

        // ─────────────────────────────────────────
        // EDIT USER
        // ─────────────────────────────────────────
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
                var r = dialog.Result;

                user.FullName = r.FullName;
                user.Email = r.Email;
                user.Phone = r.PhoneNumber;
                user.ClinicName = r.ClinicName;

                if (r.Password is not null)
                    user.Password = r.Password;

                _vm.ApplyFilters();
                UpdateKpiCards();
            }
        }

        // ─────────────────────────────────────────
        // MANAGE MODULES
        // ─────────────────────────────────────────
        private async void ManageModules_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
            int userId = (int)item.Tag;

            var user = _vm.FindUser(userId);
            if (user == null) return;

            var dialog = new ManageModules(user) { XamlRoot = XamlRoot };
            await dialog.ShowAsync();
        }

        // ─────────────────────────────────────────
        // DEACTIVATE
        // ─────────────────────────────────────────
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
                _vm.DeactivateUser(userId);
                UpdateKpiCards();
            }
        }

        // ─────────────────────────────────────────
        // REACTIVATE
        // ─────────────────────────────────────────
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
                _vm.ReactivateUser(userId);
                UpdateKpiCards();
            }
        }

        // ─────────────────────────────────────────
        // DELETE
        // ─────────────────────────────────────────
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
                _vm.DeleteUser(userId);
                UpdateKpiCards();
            }
        }
    }
}