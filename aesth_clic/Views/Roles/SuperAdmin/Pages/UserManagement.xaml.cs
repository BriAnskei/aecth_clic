using aesth_clic.ViewModels.SuperAdmin;
using aesth_clic.Views.Roles.SuperAdmin.Modals;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using Windows.UI;

namespace aesth_clic.Views.Roles.SuperAdmin.Pages
{
    // ─────────────────────────────────────────────────────────
    // USER MODEL
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

        // ── Derived display helpers ──────────────────────────

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

        public Visibility DeactivateVisible => Status == "Active" ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ReactivateVisible => Status == "Deactivated" ? Visibility.Visible : Visibility.Collapsed;
        public Visibility DeleteVisible => Status == "Deactivated" ? Visibility.Visible : Visibility.Collapsed;

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }

    // ─────────────────────────────────────────────────────────
    // PAGE CODE-BEHIND
    // ─────────────────────────────────────────────────────────
    public sealed partial class UserManagement : Page
    {
        // ── Single source of truth ───────────────────────────
        private readonly UserManagementViewModel _vm = new();

        public UserManagement()
        {
            InitializeComponent();

            // Load mock data into the ViewModel
            _vm.LoadMockData();

            // Bind the ItemsControl to the ViewModel's DisplayedUsers collection
            UserListControl.ItemsSource = _vm.DisplayedUsers;

            // Sync KPI cards with initial state
            UpdateKpiCards();

            // Subscribe to ViewModel property changes to keep KPI cards in sync
            _vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is nameof(UserManagementViewModel.TotalUsers)
                                   or nameof(UserManagementViewModel.ActiveUsers)
                                   or nameof(UserManagementViewModel.DeactivatedUsers))
                    UpdateKpiCards();
            };
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
        // TOOLBAR EVENTS — delegate to ViewModel
        // ─────────────────────────────────────────
        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            _vm.SearchText = sender.Text;
            UpdateKpiCards(); // row count refresh
        }

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _vm.SelectedStatus =
                (StatusFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All";
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

            if (result == ContentDialogResult.Primary && dialog.Result is not null)
            {
                var r = dialog.Result;

                _vm.AddUser(new UserItem
                {
                    FullName = r.FullName,
                    Email = r.Username,
                    Phone = r.PhoneNumber,
                    ClinicName = r.ClinicName,
                    Password = r.Password,
                    Status = "Active"
                });

                UpdateKpiCards();
            }
        }

        // ─────────────────────────────────────────
        // EDIT USER
        // ─────────────────────────────────────────
        private async void EditUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item) return;
            int userId = (int)item.Tag;

            // Resolve from the ViewModel's full list via a small public helper
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

                // Re-run filters so the table reflects the edited data
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
        // DEACTIVATE  (Active → Deactivated)
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
        // REACTIVATE  (Deactivated → Active)
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
        // DELETE  (only when Deactivated)
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