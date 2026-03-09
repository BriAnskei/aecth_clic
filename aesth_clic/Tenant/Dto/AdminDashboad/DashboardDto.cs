using System.Collections.Generic;

namespace aesth_clic.Tenant.DTO
{
    public class DashboardDto
    {
        public int TotalPatients { get; set; }
        public double MonthlyRevenue { get; set; }
        public List<ProcedureStatDto> TopProcedures { get; set; } = new();
        public List<LowStockMedicineDto> LowStockMedicines { get; set; } = new();
    }

    public class ProcedureStatDto
    {
        public string ProcedureName { get; set; } = string.Empty;
        public int Count { get; set; }
        /// <summary>
        /// 0–100, relative to the most popular procedure this month.
        /// </summary>
        public int BarPercent { get; set; }
    }

    public class LowStockMedicineDto
    {
        public string Name { get; set; } = string.Empty;
        public int Stock { get; set; }
        /// <summary>
        /// Hardcoded low-stock threshold: 10
        /// </summary>
        public int Threshold { get; set; } = 10;
    }
}