using aesth_clic.Master.Dto.Dashboard;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI;

namespace aesth_clic.ViewModels.SuperAdmin
{
    public sealed class SuperAdminDashboardViewModel : INotifyPropertyChanged
    {
        // ── Revenue ───────────────────────────────────────────────────────────
        private string _monthlyRevenue = "₱0.00";
        public string MonthlyRevenue
        {
            get => _monthlyRevenue;
            private set { _monthlyRevenue = value; OnPropertyChanged(); }
        }

        // ── Client counts ─────────────────────────────────────────────────────
        private string _totalClients = "0";
        public string TotalClients
        {
            get => _totalClients;
            private set { _totalClients = value; OnPropertyChanged(); }
        }

        private string _activeClients = "0";
        public string ActiveClients
        {
            get => _activeClients;
            private set { _activeClients = value; OnPropertyChanged(); }
        }

        private string _inactiveClientsLabel = "0 inactive accounts";
        public string InactiveClientsLabel
        {
            get => _inactiveClientsLabel;
            private set { _inactiveClientsLabel = value; OnPropertyChanged(); }
        }

        private SolidColorBrush _inactiveClientsForeground =
            new(Color.FromArgb(255, 14, 164, 122));
        public SolidColorBrush InactiveClientsForeground
        {
            get => _inactiveClientsForeground;
            private set { _inactiveClientsForeground = value; OnPropertyChanged(); }
        }

        // ── Chart data (consumed directly by the page's DrawLineChart) ────────
        private List<string> _chartLabels = new();
        public List<string> ChartLabels
        {
            get => _chartLabels;
            private set { _chartLabels = value; OnPropertyChanged(); }
        }

        private List<int> _chartValues = new();
        public List<int> ChartValues
        {
            get => _chartValues;
            private set { _chartValues = value; OnPropertyChanged(); }
        }

        // ── Load ──────────────────────────────────────────────────────────────
        public void LoadFromDto(SuperAdminDashboardDto dto)
        {
            MonthlyRevenue = $"₱{dto.MonthlyRevenue:N2}";

            TotalClients = dto.TotalClients.ToString();

            ActiveClients = dto.ActiveClients.ToString();

            int inactive = dto.InactiveClients;
            InactiveClientsLabel = $"{inactive} inactive account{(inactive != 1 ? "s" : "")}";
            InactiveClientsForeground = inactive == 0
                ? new SolidColorBrush(Color.FromArgb(255, 14, 164, 122))   // green
                : new SolidColorBrush(Color.FromArgb(255, 216, 59, 1));   // red-orange

            ChartLabels = dto.ChartLabels;
            ChartValues = dto.ChartValues;
        }

        // ── INPC ──────────────────────────────────────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}