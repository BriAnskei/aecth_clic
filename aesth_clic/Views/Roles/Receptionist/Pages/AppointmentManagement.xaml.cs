using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace aesth_clic.Views.Roles.Receptionist.Pages
{
    // ── Data Model ─────────────────────────────────────────────────────────────
    public class AppointmentItem
    {
        public string AppointmentId { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string AvatarColor { get; set; } = "#5B2D8E";
        public string DoctorName { get; set; } = string.Empty;
        public string ProcedureName { get; set; } = string.Empty;
        public string AppointmentDate { get; set; } = string.Empty;
        public string AppointmentTime { get; set; } = string.Empty;
    }

    // ── Page ───────────────────────────────────────────────────────────────────
    public sealed partial class AppointmentManagement : Page
    {
        private List<AppointmentItem> _allAppointments = new();

        public AppointmentManagement()
        {
            InitializeComponent();
            LoadSampleData();
            Loaded += (_, _) => ApplyFilters();
        }

        // ── Sample Data ────────────────────────────────────────────────────────
        private void LoadSampleData()
        {
            _allAppointments = new List<AppointmentItem>
            {
                BuildItem("a1",  "p1",  "Maria Santos",    "Female", "Dr. Isabel Reyes",     "Botox Injection",      "Mar 05, 2025", "10:00 AM"),
                BuildItem("a2",  "p3",  "Ana Cruz",        "Female", "Dr. Carlos Mendoza",   "Body Contouring",      "Apr 20, 2025", "02:30 PM"),
                BuildItem("a3",  "p5",  "Liza Flores",     "Female", "Dr. Isabel Reyes",     "Dermal Fillers",       "Mar 28, 2025", "11:00 AM"),
                BuildItem("a4",  "p9",  "Grace Tan",       "Female", "Dr. Sofia Villanueva", "Lip Augmentation",     "May 02, 2025", "03:00 PM"),
                BuildItem("a5",  "p1",  "Maria Santos",    "Female", "Dr. Ramon Aquino",     "Hydra Facial",         "Jan 10, 2025", "09:00 AM"),
                BuildItem("a6",  "p3",  "Ana Cruz",        "Female", "Dr. Sofia Villanueva", "Chemical Peel",        "Feb 14, 2025", "01:00 PM"),
                BuildItem("a7",  "p6",  "Ramon Garcia",    "Male",   "Dr. Carlos Mendoza",   "Microdermabrasion",    "Dec 22, 2024", "10:30 AM"),
                BuildItem("a8",  "p10", "Kevin Lim",       "Male",   "Dr. Ramon Aquino",     "Back Massage Therapy", "Jan 30, 2025", "04:00 PM"),
                BuildItem("a9",  "p2",  "Jose Reyes",      "Male",   "Dr. Mark Delgado",     "Laser Hair Removal",   "Jun 10, 2025", "09:30 AM"),
                BuildItem("a10", "p4",  "Carlo Mendoza",   "Male",   "Dr. Isabel Reyes",     "Acne Scar Treatment",  "Jun 15, 2025", "11:30 AM"),
                BuildItem("a11", "p7",  "Sofia Aquino",    "Female", "Dr. Mark Delgado",     "Skin Brightening",     "Jun 20, 2025", "02:00 PM"),
                BuildItem("a12", "p8",  "Mark Villanueva", "Male",   "Dr. Carlos Mendoza",   "Laser Toning",         "Jul 01, 2025", "03:30 PM"),
            };
        }

        private static AppointmentItem BuildItem(
            string appointmentId, string patientId, string patientName, string gender,
            string doctorName, string procedureName, string date, string time)
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

            return new AppointmentItem
            {
                AppointmentId = appointmentId,
                PatientId = patientId,
                PatientName = patientName,
                Initials = initials.ToUpper(),
                AvatarColor = avatarColor,
                DoctorName = doctorName,
                ProcedureName = procedureName,
                AppointmentDate = date,
                AppointmentTime = time,
            };
        }

        // ── Filtering ──────────────────────────────────────────────────────────
        private void ApplyFilters()
        {
            if (AppointmentListControl is null) return;

            var search = SearchBox?.Text?.Trim().ToLower() ?? string.Empty;

            var filtered = _allAppointments.Where(a =>
                string.IsNullOrEmpty(search)
                || a.PatientName.ToLower().Contains(search)
                || a.ProcedureName.ToLower().Contains(search)
            ).ToList();

            AppointmentListControl.ItemsSource = filtered;

            if (TxtRowCount is not null)
                TxtRowCount.Text = $"Showing {filtered.Count} appointment{(filtered.Count == 1 ? "" : "s")}";
        }

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
            => ApplyFilters();

        // ── Kebab button click (no-op — flyout opens automatically) ───────────
        private void KebabMenu_Click(object sender, RoutedEventArgs e) { }

        // ── Add Appointment ────────────────────────────────────────────────────
        private async void AddAppointment_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Add Appointment",
                Content = "Add appointment form will be implemented here.",
                CloseButtonText = "Close",
                XamlRoot = XamlRoot
            };
            await dialog.ShowAsync();
        }

        // ── View ───────────────────────────────────────────────────────────────
        private async void ViewAppointment_Click(object sender, RoutedEventArgs e)
        {
            var id = (sender as MenuFlyoutItem)?.Tag?.ToString();
            var record = _allAppointments.FirstOrDefault(a => a.AppointmentId == id);
            if (record is null) return;

            var dialog = new ContentDialog
            {
                Title = record.ProcedureName,
                Content = $"Patient: {record.PatientName}\nDoctor: {record.DoctorName}\nProcedure: {record.ProcedureName}\nDate: {record.AppointmentDate}\nTime: {record.AppointmentTime}",
                CloseButtonText = "Close",
                XamlRoot = XamlRoot
            };
            await dialog.ShowAsync();
        }

        // ── Edit / Reschedule ──────────────────────────────────────────────────
        private async void EditAppointment_Click(object sender, RoutedEventArgs e)
        {
            var id = (sender as MenuFlyoutItem)?.Tag?.ToString();
            var record = _allAppointments.FirstOrDefault(a => a.AppointmentId == id);
            if (record is null) return;

            var dialog = new ContentDialog
            {
                Title = $"Edit / Reschedule — {record.ProcedureName}",
                Content = $"Edit or reschedule {record.PatientName}'s {record.ProcedureName} appointment.\nFunctionality will be implemented later.",
                CloseButtonText = "Close",
                XamlRoot = XamlRoot
            };
            await dialog.ShowAsync();
        }

        // ── Cancel ─────────────────────────────────────────────────────────────
        private async void CancelAppointment_Click(object sender, RoutedEventArgs e)
        {
            var id = (sender as MenuFlyoutItem)?.Tag?.ToString();
            var record = _allAppointments.FirstOrDefault(a => a.AppointmentId == id);
            if (record is null) return;

            var dialog = new ContentDialog
            {
                Title = "Cancel Appointment",
                Content = $"Cancel {record.PatientName}'s {record.ProcedureName} appointment?\nFunctionality will be implemented later.",
                CloseButtonText = "Back",
                PrimaryButtonText = "Confirm Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}