using System;
using System.Collections.Generic;
using System.Linq;
using aesth_clic.Utils;
using aesth_clic.Views.Roles.Admin.Modals;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace aesth_clic.Views.Roles.Admin.Pages
{
    // ── Converter: hex string → SolidColorBrush ───────────────────
    public class StringToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string hex && !string.IsNullOrWhiteSpace(hex))
            {
                try
                {
                    hex = hex.TrimStart('#');
                    byte a = 255,
                        r,
                        g,
                        b;
                    if (hex.Length == 6)
                    {
                        r = System.Convert.ToByte(hex[..2], 16);
                        g = System.Convert.ToByte(hex.Substring(2, 2), 16);
                        b = System.Convert.ToByte(hex.Substring(4, 2), 16);
                    }
                    else if (hex.Length == 8)
                    {
                        a = System.Convert.ToByte(hex[..2], 16);
                        r = System.Convert.ToByte(hex.Substring(2, 2), 16);
                        g = System.Convert.ToByte(hex.Substring(4, 2), 16);
                        b = System.Convert.ToByte(hex.Substring(6, 2), 16);
                    }
                    else
                        return new SolidColorBrush(Colors.Transparent);

                    return new SolidColorBrush(Color.FromArgb(a, r, g, b));
                }
                catch
                {
                    return new SolidColorBrush(Colors.Transparent);
                }
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            string language
        ) => throw new NotImplementedException();
    }

    // ── Converter: "Visible"/"Collapsed" string → Visibility ──────
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) =>
            value is string s && s.Equals("Visible", StringComparison.OrdinalIgnoreCase)
                ? Visibility.Visible
                : Visibility.Collapsed;

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            string language
        ) => value is Visibility v && v == Visibility.Visible ? "Visible" : "Collapsed";
    }

    // ── Data model ────────────────────────────────────────────────
    public class StaffUserItem
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string AvatarColor { get; set; } = "#5B2D8E";
        public string RoleBadgeColor { get; set; } = string.Empty;
        public string RoleBadgeText { get; set; } = string.Empty;
        public string StatusBadgeColor { get; set; } = string.Empty;
        public string StatusBadgeText { get; set; } = string.Empty;
        public string DeactivateVisible { get; set; } = "Visible";
        public string ReactivateVisible { get; set; } = "Collapsed";
        public string DeleteVisible { get; set; } = "Collapsed";
    }

    // ── Page code-behind ──────────────────────────────────────────
    public sealed partial class UserManagement : Page
    {
        private List<StaffUserItem> _allUsers = new();

        public UserManagement()
        {
            InitializeComponent();
            LoadSampleData();
            Loaded += (_, _) => ApplyFilters();
        }

        // ── Sample / seed data ────────────────────────────────────
        private void LoadSampleData()
        {
            _allUsers = new List<StaffUserItem>
            {
                BuildItem(
                    "u1",
                    "Dr. Maria Santos",
                    "maria@clinic.com",
                    "0912-345-6789",
                    "Doctor",
                    "Active"
                ),
                BuildItem(
                    "u2",
                    "Jose Reyes",
                    "jose@clinic.com",
                    "0923-456-7890",
                    "Receptionist",
                    "Active"
                ),
                BuildItem(
                    "u3",
                    "Ana Cruz",
                    "ana@clinic.com",
                    "0934-567-8901",
                    "Pharmacist",
                    "Active"
                ),
                BuildItem(
                    "u4",
                    "Dr. Carlo Mendoza",
                    "carlo@clinic.com",
                    "0945-678-9012",
                    "Doctor",
                    "Deactivated"
                ),
                BuildItem(
                    "u5",
                    "Liza Flores",
                    "liza@clinic.com",
                    "0956-789-0123",
                    "Receptionist",
                    "Deactivated"
                ),
            };
        }

        // ── Factory helper ────────────────────────────────────────
        private static StaffUserItem BuildItem(
            string id,
            string name,
            string email,
            string phone,
            string role,
            string status
        )
        {
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var initials =
                parts.Length >= 2 ? $"{parts[0][0]}{parts[^1][0]}"
                : name.Length > 0 ? name[0].ToString()
                : "?";

            var avatarColor = role switch
            {
                "Doctor" => "#5B2D8E",
                "Receptionist" => "#0078D4",
                "Pharmacist" => "#E67E22",
                _ => "#888888",
            };

            var (roleBg, roleFg) = role switch
            {
                "Doctor" => ("#EDE7F6", "#5B2D8E"),
                "Receptionist" => ("#E3F2FD", "#0078D4"),
                "Pharmacist" => ("#FEF3E2", "#E67E22"),
                _ => ("#F0F0F0", "#555555"),
            };

            bool isActive = status == "Active";

            return new StaffUserItem
            {
                UserId = id,
                FullName = name,
                Email = email,
                Phone = phone,
                Role = role,
                Status = status,
                Initials = initials.ToUpper(),
                AvatarColor = avatarColor,
                RoleBadgeColor = roleBg,
                RoleBadgeText = roleFg,
                StatusBadgeColor = isActive ? "#E6F4F1" : "#FBE9E7",
                StatusBadgeText = isActive ? "#0EA47A" : "#D83B01",
                DeactivateVisible = isActive ? "Visible" : "Collapsed",
                ReactivateVisible = !isActive ? "Visible" : "Collapsed",
                DeleteVisible = !isActive ? "Visible" : "Collapsed",
            };
        }

        // ── Filtering ─────────────────────────────────────────────
        private void ApplyFilters()
        {
            if (UserListControl is null)
                return; // ← safety guard

            var search = SearchBox?.Text?.Trim().ToLower() ?? string.Empty;
            var roleTag = (RoleFilter?.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All";
            var statusTag = (StatusFilter?.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All";

            var filtered = _allUsers
                .Where(u =>
                {
                    bool matchSearch =
                        string.IsNullOrEmpty(search)
                        || u.FullName.ToLower().Contains(search)
                        || u.Email.ToLower().Contains(search)
                        || u.Phone.Contains(search);
                    bool matchRole = roleTag == "All" || u.Role == roleTag;
                    bool matchStatus = statusTag == "All" || u.Status == statusTag;
                    return matchSearch && matchRole && matchStatus;
                })
                .ToList();

            UserListControl.ItemsSource = filtered;

            if (TxtTotalUsers is not null)
                TxtTotalUsers.Text = _allUsers.Count.ToString();
            if (TxtActiveUsers is not null)
                TxtActiveUsers.Text = _allUsers.Count(u => u.Status == "Active").ToString();
            if (TxtInactiveUsers is not null)
                TxtInactiveUsers.Text = _allUsers.Count(u => u.Status == "Deactivated").ToString();
            if (TxtRowCount is not null)
                TxtRowCount.Text =
                    $"Showing {filtered.Count} user{(filtered.Count == 1 ? "" : "s")}";
        }

        // ── Toolbar event handlers ────────────────────────────────
        private void SearchBox_TextChanged(
            AutoSuggestBox sender,
            AutoSuggestBoxTextChangedEventArgs args
        ) => ApplyFilters();

        private void RoleFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            ApplyFilters();

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            ApplyFilters();

        // ── Add User ──────────────────────────────────────────────
        private async void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddNewUser { XamlRoot = XamlRoot };
            await dialog.ShowAsync();

            if (dialog.Result is null)
                return;

            var r = dialog.Result;

            var newUser = BuildItem(
                id: Guid.NewGuid().ToString(),
                name: r.FullName,
                email: r.Email,
                phone: r.Phone,
                role: r.Role,
                status: "Active"
            );

            _allUsers.Add(newUser);
            ApplyFilters();
            ToastHelper.Success(
                ToastBar,
                "User added",
                $"{r.FullName} has been added as {r.Role}."
            );
        }

        // ── Edit ──────────────────────────────────────────────────
        private async void EditUser_Click(object sender, RoutedEventArgs e)
        {
            var userId = (sender as Button)?.Tag?.ToString();
            var user = _allUsers.FirstOrDefault(u => u.UserId == userId);
            if (user is null)
                return;

            // TODO: replace with your real Edit dialog
            var dialog = new ContentDialog
            {
                Title = $"Edit — {user.FullName}",
                Content = $"Edit dialog for user ID: {userId}",
                CloseButtonText = "Cancel",
                XamlRoot = XamlRoot,
            };
            await dialog.ShowAsync();
        }

        // ── Deactivate ────────────────────────────────────────────
        private async void DeactivateUser_Click(object sender, RoutedEventArgs e)
        {
            var userId = (sender as Button)?.Tag?.ToString();
            var user = _allUsers.FirstOrDefault(u => u.UserId == userId);
            if (user is null)
                return;
            var dlg = new DeactivateUser(user);
            dlg.XamlRoot = XamlRoot;
            var res = await dlg.ShowAsync();
            if (dlg.Confirmed)
            {
                user.Status = "Deactivated";
                user.DeactivateVisible = "Collapsed";
                user.ReactivateVisible = "Visible";
                user.DeleteVisible = "Visible";
                user.StatusBadgeColor = "#FBE9E7";
                user.StatusBadgeText = "#D83B01";

                ApplyFilters();
                ToastHelper.Warning(
                    ToastBar,
                    "User deactivated",
                    $"{user.FullName} has been deactivated."
                );
            }
        }

        // ── Reactivate ────────────────────────────────────────────
        private async void ReactivateUser_Click(object sender, RoutedEventArgs e)
        {
            var userId = (sender as Button)?.Tag?.ToString();
            var user = _allUsers.FirstOrDefault(u => u.UserId == userId);
            if (user is null)
                return;
            var dlg = new ReactivateUser(user);
            dlg.XamlRoot = XamlRoot;
            var res = await dlg.ShowAsync();
            if (dlg.Confirmed)
            {
                user.Status = "Active";
                user.DeactivateVisible = "Visible";
                user.ReactivateVisible = "Collapsed";
                user.DeleteVisible = "Collapsed";
                user.StatusBadgeColor = "#E6F4F1";
                user.StatusBadgeText = "#0EA47A";

                ApplyFilters();
                ToastHelper.Success(
                    ToastBar,
                    "User reactivated",
                    $"{user.FullName} has been reactivated."
                );
            }
        }

        // ── Delete ────────────────────────────────────────────────
        private async void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            var userId = (sender as Button)?.Tag?.ToString();
            var user = _allUsers.FirstOrDefault(u => u.UserId == userId);
            if (user is null)
                return;

            var confirm = new ContentDialog
            {
                Title = "Delete User",
                Content = $"Permanently delete {user.FullName}? This cannot be undone.",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = XamlRoot,
            };

            if (await confirm.ShowAsync() != ContentDialogResult.Primary)
                return;

            _allUsers.Remove(user);
            ApplyFilters();
        }
    }
}
