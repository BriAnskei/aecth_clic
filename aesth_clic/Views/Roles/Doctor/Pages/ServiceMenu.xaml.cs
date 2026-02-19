using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace aesth_clic.Views.Roles.Doctor.Pages
{
    public class ServiceMenuItem
    {
        public string ServiceId { get; set; } = string.Empty;
        public string ProcedureName { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;
        public string AddedByDoctor { get; set; } = string.Empty;
        public string DoctorInitials { get; set; } = string.Empty;
        public string DoctorAvatarColor { get; set; } = "#5B2D8E";
        public decimal RawPrice { get; set; }
    }

    public sealed partial class ServiceMenu : Page
    {
        private List<ServiceMenuItem> _allServices = new();

        public ServiceMenu()
        {
            InitializeComponent();
            LoadSampleData();
            Loaded += (_, _) => ApplyFilters();
        }

        private void LoadSampleData()
        {
            _allServices = new List<ServiceMenuItem>
            {
                BuildItem("s1",  "Hydra Facial",         3500m,  "Dr. Maria Santos"),
                BuildItem("s2",  "Botox Injection",       8000m,  "Dr. Jose Reyes"),
                BuildItem("s3",  "Laser Hair Removal",    5200m,  "Dr. Ana Cruz"),
                BuildItem("s4",  "Chemical Peel",         2800m,  "Dr. Maria Santos"),
                BuildItem("s5",  "Body Contouring",      12000m,  "Dr. Jose Reyes"),
                BuildItem("s6",  "Acne Scar Treatment",   6500m,  "Dr. Ana Cruz"),
                BuildItem("s7",  "Dermal Fillers",        9500m,  "Dr. Maria Santos"),
                BuildItem("s8",  "Microdermabrasion",     2200m,  "Dr. Jose Reyes"),
                BuildItem("s9",  "Skin Brightening",      3900m,  "Dr. Ana Cruz"),
                BuildItem("s10", "Laser Toning",          4800m,  "Dr. Maria Santos"),
                BuildItem("s11", "Lip Augmentation",      7200m,  "Dr. Jose Reyes"),
                BuildItem("s12", "Back Massage Therapy",  1800m,  "Dr. Ana Cruz"),
            };
        }

        private static ServiceMenuItem BuildItem(
            string id, string name, decimal rawPrice, string doctor)
        {
            var parts = doctor.Replace("Dr. ", "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var initials = parts.Length >= 2
                ? $"{parts[0][0]}{parts[^1][0]}"
                : parts.Length > 0 ? parts[0][0].ToString() : "?";

            var avatarColor = doctor switch
            {
                var d when d.Contains("Maria") => "#C2185B",
                var d when d.Contains("Jose") => "#0078D4",
                var d when d.Contains("Ana") => "#5B2D8E",
                _ => "#5B2D8E"
            };

            return new ServiceMenuItem
            {
                ServiceId = id,
                ProcedureName = name,
                Price = $"₱{rawPrice:N0}",
                RawPrice = rawPrice,
                AddedByDoctor = doctor,
                DoctorInitials = initials.ToUpper(),
                DoctorAvatarColor = avatarColor,
            };
        }

        private void ApplyFilters()
        {
            if (ServiceListControl is null) return;

            var search = SearchBox?.Text?.Trim().ToLower() ?? string.Empty;

            var filtered = _allServices.Where(s =>
                string.IsNullOrEmpty(search)
                || s.ProcedureName.ToLower().Contains(search)
                || s.AddedByDoctor.ToLower().Contains(search)
            ).ToList();

            ServiceListControl.ItemsSource = filtered;

            if (TxtTotalServices is not null)
                TxtTotalServices.Text = _allServices.Count.ToString();

            if (TxtAvgPrice is not null)
            {
                var avg = _allServices.Count > 0 ? _allServices.Average(s => s.RawPrice) : 0m;
                TxtAvgPrice.Text = $"₱{avg:N0}";
            }

            if (TxtRowCount is not null)
                TxtRowCount.Text = $"Showing {filtered.Count} service{(filtered.Count == 1 ? "" : "s")}";
        }

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
            => ApplyFilters();

        private async void AddServiceButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Add Service",
                Content = "Add Service dialog will be implemented here.",
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Save",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = XamlRoot
            };
            await dialog.ShowAsync();
        }

        private async void ViewService_Click(object sender, RoutedEventArgs e)
        {
            var serviceId = (sender as Button)?.Tag?.ToString();
            var record = _allServices.FirstOrDefault(s => s.ServiceId == serviceId);
            if (record is null) return;

            var dialog = new ContentDialog
            {
                Title = record.ProcedureName,
                Content = $"Price: {record.Price}\nAdded By: {record.AddedByDoctor}",
                CloseButtonText = "Close",
                XamlRoot = XamlRoot
            };
            await dialog.ShowAsync();
        }

        private async void EditService_Click(object sender, RoutedEventArgs e)
        {
            var serviceId = (sender as Button)?.Tag?.ToString();
            var record = _allServices.FirstOrDefault(s => s.ServiceId == serviceId);
            if (record is null) return;

            var dialog = new ContentDialog
            {
                Title = $"Edit — {record.ProcedureName}",
                Content = "Edit Service dialog will be implemented here.",
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Save",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}