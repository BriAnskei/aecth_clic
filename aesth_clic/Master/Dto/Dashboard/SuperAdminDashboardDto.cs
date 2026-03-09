using System.Collections.Generic;

namespace aesth_clic.Master.Dto.Dashboard
{
    public class SuperAdminDashboardDto
    {
        public decimal MonthlyRevenue { get; set; }
        public int TotalClients { get; set; }
        public int ActiveClients { get; set; }
        public int InactiveClients { get; set; }

        /// <summary>Month labels for the last 12 months, e.g. ["Mar","Apr",…]</summary>
        public List<string> ChartLabels { get; set; } = new();

        /// <summary>New-clinic counts aligned to ChartLabels.</summary>
        public List<int> ChartValues { get; set; } = new();
    }
}