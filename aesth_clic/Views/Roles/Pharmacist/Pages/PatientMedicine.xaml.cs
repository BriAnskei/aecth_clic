using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace aesth_clic.Views.Roles.Pharmacist.Pages
{
    // ── Converter ────────────────────────────────────────────────────────────
    public class StringToBrushConverter : Microsoft.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string hex)
            {
                hex = hex.TrimStart('#');
                if (hex.Length == 6)
                {
                    byte r = System.Convert.ToByte(hex.Substring(0, 2), 16);
                    byte g = System.Convert.ToByte(hex.Substring(2, 2), 16);
                    byte b = System.Convert.ToByte(hex.Substring(4, 2), 16);
                    return new SolidColorBrush(Color.FromArgb(255, r, g, b));
                }
            }
            return new SolidColorBrush(Color.FromArgb(255, 91, 45, 142));
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    // ── Data model ───────────────────────────────────────────────────────────
    public class PatientMedicineItem
    {
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string AvatarColor { get; set; } = "#5B2D8E";
        public string AssignedDoctor { get; set; } = string.Empty;
        public int TotalMedicine { get; set; }
        public string TotalMedicineDisplay => $"{TotalMedicine} medicine{(TotalMedicine == 1 ? "" : "s")}";
    }

    // ── Page ─────────────────────────────────────────────────────────────────
    public sealed partial class PatientMedicine : Page
    {
        private List<PatientMedicineItem> _allItems = new();

        public PatientMedicine()
        {
            InitializeComponent();
            LoadSampleData();
            Loaded += (_, _) => ApplyFilters();
        }

        private void LoadSampleData()
        {
            _allItems = new List<PatientMedicineItem>
            {
                BuildItem("p1",  "Maria Santos",    "Female", "Dr. Reyes",    4),
                BuildItem("p2",  "Jose Reyes",      "Male",   "Dr. Santos",   2),
                BuildItem("p3",  "Ana Cruz",        "Female", "Dr. Lim",      5),
                BuildItem("p4",  "Carlo Mendoza",   "Male",   "Dr. Reyes",    3),
                BuildItem("p5",  "Liza Flores",     "Female", "Dr. Garcia",   6),
                BuildItem("p6",  "Ramon Garcia",    "Male",   "Dr. Santos",   1),
                BuildItem("p7",  "Sofia Aquino",    "Female", "Dr. Lim",      3),
                BuildItem("p8",  "Mark Villanueva", "Male",   "Dr. Garcia",   2),
                BuildItem("p9",  "Grace Tan",       "Female", "Dr. Reyes",    4),
                BuildItem("p10", "Kevin Lim",       "Male",   "Dr. Santos",   3),
            };
        }

        private static PatientMedicineItem BuildItem(
            string patientId, string patientName, string gender,
            string assignedDoctor, int totalMedicine)
        {
            var parts = patientName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var initials = parts.Length >= 2
                ? $"{parts[0][0]}{parts[^1][0]}"
                : patientName.Length > 0 ? patientName[0].ToString() : "?";

            var avatarColor = gender switch
            {
                "Female" => "#C2185B",
                "Male" => "#0078D4",
                _ => "#5B2D8E"
            };

            return new PatientMedicineItem
            {
                PatientId = patientId,
                PatientName = patientName,
                Initials = initials.ToUpper(),
                AvatarColor = avatarColor,
                AssignedDoctor = assignedDoctor,
                TotalMedicine = totalMedicine,
            };
        }

        private void ApplyFilters()
        {
            if (PatientMedicineListControl is null) return;

            var search = SearchBox?.Text?.Trim().ToLower() ?? string.Empty;

            var filtered = _allItems.Where(p =>
                string.IsNullOrEmpty(search)
                || p.PatientName.ToLower().Contains(search)
                || p.AssignedDoctor.ToLower().Contains(search)
            ).ToList();

            PatientMedicineListControl.ItemsSource = filtered;

            if (TxtRowCount is not null)
                TxtRowCount.Text = $"Showing {filtered.Count} patient{(filtered.Count == 1 ? "" : "s")}";
        }

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
            => ApplyFilters();

        private void KebabMenu_Click(object sender, RoutedEventArgs e) { /* flyout handles itself */ }

        private async void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            var id = (sender as MenuFlyoutItem)?.Tag?.ToString();
            var record = _allItems.FirstOrDefault(p => p.PatientId == id);
            if (record is null) return;

            var dialog = new ContentDialog
            {
                Title = $"{record.PatientName} — Medicine Details",
                Content = "Detailed medicine information will be implemented later.",
                CloseButtonText = "Close",
                XamlRoot = XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}