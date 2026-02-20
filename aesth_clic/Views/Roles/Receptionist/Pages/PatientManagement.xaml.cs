using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace aesth_clic.Views.Roles.Receptionist.Pages
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
                    byte a = 255, r, g, b;
                    if (hex.Length == 6)
                    {
                        r = System.Convert.ToByte(hex.Substring(0, 2), 16);
                        g = System.Convert.ToByte(hex.Substring(2, 2), 16);
                        b = System.Convert.ToByte(hex.Substring(4, 2), 16);
                    }
                    else if (hex.Length == 8)
                    {
                        a = System.Convert.ToByte(hex.Substring(0, 2), 16);
                        r = System.Convert.ToByte(hex.Substring(2, 2), 16);
                        g = System.Convert.ToByte(hex.Substring(4, 2), 16);
                        b = System.Convert.ToByte(hex.Substring(6, 2), 16);
                    }
                    else return new SolidColorBrush(Colors.Transparent);

                    return new SolidColorBrush(Color.FromArgb(a, r, g, b));
                }
                catch { return new SolidColorBrush(Colors.Transparent); }
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    // ── Data model ────────────────────────────────────────────────
    public class PatientItem
    {
        public string PatientId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Birthday { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string AvatarColor { get; set; } = "#5B2D8E";
        public string GenderBadgeColor { get; set; } = string.Empty;
        public string GenderBadgeText { get; set; } = string.Empty;
    }

    // ── Page code-behind ──────────────────────────────────────────
    public sealed partial class PatientManagement : Page
    {
        private List<PatientItem> _allPatients = new();

        public PatientManagement()
        {
            InitializeComponent();
            LoadSampleData();
            Loaded += (_, _) => ApplyFilters();
        }

        // ── Sample / seed data ────────────────────────────────────
        private void LoadSampleData()
        {
            _allPatients = new List<PatientItem>
            {
                BuildItem("p1",  "Maria Santos",   "maria.santos@gmail.com",    "0912-345-6789", "Female", "Mar 12, 1990"),
                BuildItem("p2",  "Jose Reyes",     "jose.reyes@gmail.com",      "0923-456-7890", "Male",   "Jul 04, 1985"),
                BuildItem("p3",  "Ana Cruz",        "ana.cruz@gmail.com",        "0934-567-8901", "Female", "Nov 22, 1998"),
                BuildItem("p4",  "Carlo Mendoza",   "carlo.mendoza@gmail.com",   "0945-678-9012", "Male",   "Jan 15, 1979"),
                BuildItem("p5",  "Liza Flores",     "liza.flores@gmail.com",     "0956-789-0123", "Female", "Sep 08, 2000"),
                BuildItem("p6",  "Ramon Garcia",    "ramon.garcia@gmail.com",    "0967-890-1234", "Male",   "Feb 28, 1992"),
                BuildItem("p7",  "Sofia Aquino",    "sofia.aquino@gmail.com",    "0978-901-2345", "Female", "Jun 17, 1995"),
                BuildItem("p8",  "Mark Villanueva", "mark.villanueva@gmail.com", "0989-012-3456", "Male",   "Dec 03, 1988"),
                BuildItem("p9",  "Grace Tan",       "grace.tan@gmail.com",       "0990-123-4567", "Female", "Apr 30, 2002"),
                BuildItem("p10", "Kevin Lim",       "kevin.lim@gmail.com",       "0911-234-5678", "Male",   "Aug 19, 1983"),
            };
        }

        // ── Factory helper ────────────────────────────────────────
        private static PatientItem BuildItem(
            string id, string name, string email, string phone,
            string gender, string birthday)
        {
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var initials = parts.Length >= 2
                ? $"{parts[0][0]}{parts[^1][0]}"
                : name.Length > 0 ? name[0].ToString() : "?";

            var avatarColor = gender switch
            {
                "Female" => "#C2185B",
                "Male" => "#0078D4",
                _ => "#5B2D8E"
            };

            var (genderBg, genderFg) = gender switch
            {
                "Female" => ("#FCE4EC", "#C2185B"),
                "Male" => ("#E3F2FD", "#0078D4"),
                _ => ("#EDE4F9", "#5B2D8E")
            };

            return new PatientItem
            {
                PatientId = id,
                FullName = name,
                Email = email,
                Phone = phone,
                Gender = gender,
                Birthday = birthday,
                Initials = initials.ToUpper(),
                AvatarColor = avatarColor,
                GenderBadgeColor = genderBg,
                GenderBadgeText = genderFg,
            };
        }

        // ── Filtering + Sorting ───────────────────────────────────
        private void ApplyFilters()
        {
            if (PatientListControl is null) return;

            var search = SearchBox?.Text?.Trim().ToLower() ?? string.Empty;
            var genderTag = (GenderFilter?.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All";
            var sortTag = (SortOrder?.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "AZ";

            var filtered = _allPatients.Where(p =>
            {
                bool matchSearch = string.IsNullOrEmpty(search)
                    || p.FullName.ToLower().Contains(search)
                    || p.Email.ToLower().Contains(search)
                    || p.Phone.Contains(search);
                bool matchGender = genderTag == "All" || p.Gender == genderTag;
                return matchSearch && matchGender;
            });

            // FIX 2: SortOrder now actually applied
            filtered = sortTag == "ZA"
                ? filtered.OrderByDescending(p => p.FullName)
                : filtered.OrderBy(p => p.FullName);

            var result = filtered.ToList();

            PatientListControl.ItemsSource = result;

            if (TxtTotalPatients is not null) TxtTotalPatients.Text = _allPatients.Count.ToString();
            if (TxtMalePatients is not null) TxtMalePatients.Text = _allPatients.Count(p => p.Gender == "Male").ToString();
            if (TxtFemalePatients is not null) TxtFemalePatients.Text = _allPatients.Count(p => p.Gender == "Female").ToString();
            if (TxtRowCount is not null) TxtRowCount.Text = $"Showing {result.Count} patient{(result.Count == 1 ? "" : "s")}";
        }

        // ── Toolbar event handlers ────────────────────────────────

        // FIX 1: Was AutoSuggestBoxTextChangedEventArgs — must be TextChangedEventArgs for TextBox
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
            => ApplyFilters();

        private void GenderFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => ApplyFilters();

        // FIX 2: Was missing entirely
        private void SortOrder_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => ApplyFilters();

        // ── Add Patient ───────────────────────────────────────────
        private async void AddPatientButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Add Patient",
                Content = "Add Patient dialog goes here.",
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Save",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = XamlRoot
            };
            await dialog.ShowAsync();
        }

        // ── Edit ──────────────────────────────────────────────────
        // FIX 3: Was casting sender to Button — must be MenuFlyoutItem
        private async void EditPatient_Click(object sender, RoutedEventArgs e)
        {
            var patientId = (sender as MenuFlyoutItem)?.Tag?.ToString();
            var patient = _allPatients.FirstOrDefault(p => p.PatientId == patientId);
            if (patient is null) return;

            var dialog = new ContentDialog
            {
                Title = $"Edit — {patient.FullName}",
                Content = $"Edit dialog for patient ID: {patientId}",
                CloseButtonText = "Cancel",
                XamlRoot = XamlRoot
            };
            await dialog.ShowAsync();
        }

        // ── Delete ────────────────────────────────────────────────
        // FIX 3: Was casting sender to Button — must be MenuFlyoutItem
        private async void DeletePatient_Click(object sender, RoutedEventArgs e)
        {
            var patientId = (sender as MenuFlyoutItem)?.Tag?.ToString();
            var patient = _allPatients.FirstOrDefault(p => p.PatientId == patientId);
            if (patient is null) return;

            var confirm = new ContentDialog
            {
                Title = "Delete Patient",
                Content = $"Permanently delete {patient.FullName}? This cannot be undone.",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = XamlRoot
            };

            if (await confirm.ShowAsync() != ContentDialogResult.Primary) return;

            _allPatients.Remove(patient);
            ApplyFilters();
        }
    }
}