using aesth_clic.Tenant.DTO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace aesth_clic.ViewModels.Admin
{
    // ── UI display model for a procedure stat row ─────────────────────────────
    public class ProcedureStatItem
    {
        public string ProcedureName { get; set; } = string.Empty;
        public int Count { get; set; }
        public int BarPercent { get; set; }
        // Progress bar value: 0–100 relative to the top procedure this month
    }

    // ── UI display model for a low-stock medicine row ─────────────────────────
    public class LowStockItem
    {
        public string Name { get; set; } = string.Empty;
        public string StockDisplay { get; set; } = string.Empty;   // e.g. "3 units"
        public string ThresholdDisplay { get; set; } = string.Empty; // e.g. "10 units"
        // Dot color: red when stock <= 5, orange when 6–9
        public Windows.UI.Color DotColor { get; set; }
    }

    // ── ViewModel ─────────────────────────────────────────────────────────────
    public class AdminDashboardViewModel : INotifyPropertyChanged
    {
        // ── KPI values ────────────────────────────────────────────────────────
        private string _totalPatients = "—";
        public string TotalPatients
        {
            get => _totalPatients;
            set { _totalPatients = value; OnPropertyChanged(); }
        }

        private string _monthlyRevenue = "—";
        public string MonthlyRevenue
        {
            get => _monthlyRevenue;
            set { _monthlyRevenue = value; OnPropertyChanged(); }
        }

        // ── Collections ───────────────────────────────────────────────────────
        public ObservableCollection<ProcedureStatItem> TopProcedures { get; } = new();
        public ObservableCollection<LowStockItem> LowStockMedicines { get; } = new();

        // ── Derived ───────────────────────────────────────────────────────────
        public int LowStockCount => LowStockMedicines.Count;

        public string LowStockCountDisplay =>
            $"{LowStockCount} item{(LowStockCount != 1 ? "s" : "")}";

        // ── Load ──────────────────────────────────────────────────────────────
        public void LoadFromDto(DashboardDto dto)
        {
            // KPIs
            TotalPatients = dto.TotalPatients.ToString("N0");
            MonthlyRevenue = $"₱{dto.MonthlyRevenue:N2}";

            // Top procedures
            TopProcedures.Clear();
            foreach (var p in dto.TopProcedures)
                TopProcedures.Add(new ProcedureStatItem
                {
                    ProcedureName = p.ProcedureName,
                    Count = p.Count,
                    BarPercent = p.BarPercent
                });

            // Low stock medicines
            LowStockMedicines.Clear();
            foreach (var m in dto.LowStockMedicines)
            {
                // Red dot for critically low (≤ 5), orange for warning (6–9)
                var dot = m.Stock <= 5
                    ? Windows.UI.Color.FromArgb(255, 231, 76, 60)   // #E74C3C
                    : Windows.UI.Color.FromArgb(255, 230, 126, 34); // #E67E22

                LowStockMedicines.Add(new LowStockItem
                {
                    Name = m.Name,
                    StockDisplay = $"{m.Stock} units",
                    ThresholdDisplay = $"{m.Threshold} units",
                    DotColor = dot
                });
            }

            OnPropertyChanged(nameof(LowStockCount));
            OnPropertyChanged(nameof(LowStockCountDisplay));
        }

        // ── INotifyPropertyChanged ────────────────────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}