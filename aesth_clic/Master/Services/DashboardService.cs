using aesth_clic.Context;
using aesth_clic.Master.Dto.Dashboard;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace aesth_clic.Master.Services
{
    public sealed class DashboardService(MasterDbContext masterDb)
    {
        private readonly MasterDbContext _masterDb = masterDb
            ?? throw new ArgumentNullException(nameof(masterDb));

        public async Task<SuperAdminDashboardDto> GetSuperAdminDashboardAsync()
        {
            var now = DateTime.UtcNow;
            var currentMonth = now.Month;
            var currentYear = now.Year;

            // ── Revenue: SUM of subscriptions whose StartDate falls in current month ──
            var monthlyRevenue = await _masterDb.Subscription
                .Where(s => s.StartDate.Month == currentMonth
                         && s.StartDate.Year == currentYear)
                .SumAsync(s => (decimal?)s.MonthlyAmount) ?? 0m;

            // ── Client counts ──────────────────────────────────────────────────
            var allClients = await _masterDb.Clients
                .Select(c => new { c.Status, c.CreatedAt })
                .ToListAsync();

            int totalClients = allClients.Count;
            int activeClients = allClients.Count(c =>
                c.Status.Equals("active", StringComparison.OrdinalIgnoreCase));
            int inactiveClients = totalClients - activeClients;

            // ── Chart: new clinics per month for the last 12 months ───────────
            // Build the 12 month buckets starting from 11 months ago → current month
            var chartLabels = new List<string>();
            var chartValues = new List<int>();

            for (int i = 11; i >= 0; i--)
            {
                var bucket = now.AddMonths(-i);
                var bucketMonth = bucket.Month;
                var bucketYear = bucket.Year;

                int count = allClients.Count(c =>
                    c.CreatedAt.Month == bucketMonth &&
                    c.CreatedAt.Year == bucketYear);

                chartLabels.Add(bucket.ToString("MMM"));   // e.g. "Mar"
                chartValues.Add(count);
            }

            return new SuperAdminDashboardDto
            {
                MonthlyRevenue = monthlyRevenue,
                TotalClients = totalClients,
                ActiveClients = activeClients,
                InactiveClients = inactiveClients,
                ChartLabels = chartLabels,
                ChartValues = chartValues,
            };
        }
    }
}