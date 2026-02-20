using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace aesth_clic.Views.Roles.Receptionist.Pages
{
    // ── Data model ────────────────────────────────────────────────
    public class ServiceItem
    {
        public string ServiceId { get; set; } = string.Empty;
        public string ProcedureName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string FormattedPrice { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
    }

    // ── Page code-behind ──────────────────────────────────────────
    public sealed partial class ServiceMenu : Page
    {
        private List<ServiceItem> _allServices = new();

        public ServiceMenu()
        {
            InitializeComponent();
            LoadSampleData();
            PopulateDoctorFilter();
            Loaded += (_, _) => ApplyFilters();
        }

        // ── Sample data ───────────────────────────────────────────
        private void LoadSampleData()
        {
            _allServices = new List<ServiceItem>
            {
                Build("s1",  "Facial Rejuvenation",       1500m,  "Dr. Maria Santos"),
                Build("s2",  "Botox Injection",           3500m,  "Dr. Maria Santos"),
                Build("s3",  "Chemical Peel",              800m,  "Dr. Jose Reyes"),
                Build("s4",  "Dermal Filler",             5000m,  "Dr. Jose Reyes"),
                Build("s5",  "Laser Hair Removal",        2500m,  "Dr. Ana Cruz"),
                Build("s6",  "Microdermabrasion",          600m,  "Dr. Ana Cruz"),
                Build("s7",  "Hydrafacial",               1200m,  "Dr. Carlo Mendoza"),
                Build("s8",  "Teeth Whitening",           1800m,  "Dr. Carlo Mendoza"),
                Build("s9",  "PRP Hair Treatment",        7500m,  "Dr. Maria Santos"),
                Build("s10", "Skin Tag Removal",           350m,  "Dr. Jose Reyes"),
                Build("s11", "Acne Scar Treatment",       4200m,  "Dr. Ana Cruz"),
                Build("s12", "Eyebrow Threading",          250m,  "Dr. Carlo Mendoza"),
            };
        }

        private static ServiceItem Build(string id, string name, decimal price, string doctor) =>
            new ServiceItem
            {
                ServiceId = id,
                ProcedureName = name,
                Price = price,
                FormattedPrice = $"₱{price:N0}",
                DoctorName = doctor,
            };

        // ── Populate doctor filter dynamically from data ──────────
        private void PopulateDoctorFilter()
        {
            var doctors = _allServices
                .Select(s => s.DoctorName)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            foreach (var doctor in doctors)
            {
                DoctorFilter.Items.Add(new ComboBoxItem
                {
                    Content = doctor,
                    Tag = doctor
                });
            }

            DoctorFilter.SelectedIndex = 0; // "All Doctors"
        }

        // ── Filtering logic ───────────────────────────────────────
        private void ApplyFilters()
        {
            if (ServiceListControl is null) return;

            var search = SearchBox?.Text?.Trim().ToLower() ?? string.Empty;
            var doctorTag = (DoctorFilter?.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All";
            var priceTag = (PriceFilter?.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All";

            var filtered = _allServices.Where(s =>
            {
                bool matchSearch = string.IsNullOrEmpty(search)
                    || s.ProcedureName.ToLower().Contains(search);

                bool matchDoctor = doctorTag == "All" || s.DoctorName == doctorTag;

                bool matchPrice = priceTag switch
                {
                    "U500" => s.Price < 500m,
                    "500to2000" => s.Price >= 500m && s.Price <= 2000m,
                    "2000to5000" => s.Price > 2000m && s.Price <= 5000m,
                    "A5000" => s.Price > 5000m,
                    _ => true
                };

                return matchSearch && matchDoctor && matchPrice;
            }).ToList();

            ServiceListControl.ItemsSource = filtered;

            // KPI cards (always reflect full dataset)
            if (TxtTotalServices is not null)
                TxtTotalServices.Text = _allServices.Count.ToString();

            if (TxtLowestPrice is not null)
                TxtLowestPrice.Text = _allServices.Any()
                    ? $"₱{_allServices.Min(s => s.Price):N0}"
                    : "₱0";

            if (TxtHighestPrice is not null)
                TxtHighestPrice.Text = _allServices.Any()
                    ? $"₱{_allServices.Max(s => s.Price):N0}"
                    : "₱0";

            if (TxtRowCount is not null)
                TxtRowCount.Text = $"Showing {filtered.Count} service{(filtered.Count == 1 ? "" : "s")}";
        }

        // ── Event handlers ────────────────────────────────────────
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
            => ApplyFilters();

        private void DoctorFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => ApplyFilters();

        private void PriceFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => ApplyFilters();
    }
}