using aesth_clic.Session;
using aesth_clic.Tenant.DTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace aesth_clic.Tenant.Services
{
    public sealed class MasterDashboardService(TenantDbContextFactory tenantFactory) : TenantServiceBase
    {
        private const int LowStockThreshold = 10;

        private readonly TenantDbContextFactory _tenantFactory = tenantFactory;

        public async Task<DashboardDto> GetDashboardDataAsync()
        {
            using var db = CreateTenantDb();

            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd = monthStart.AddMonths(1);

            // ── 1. Total patients ─────────────────────────────────────────────
            var totalPatients = await db.Patients.CountAsync();

            // ── 2. Monthly revenue ────────────────────────────────────────────
            // Sum ServiceMenu.Price for ProcedurePayments where Status = "paid" this month.
            // PatientProcedure.CreatedAt is used as the payment date proxy since
            // ProcedurePayment has no timestamp field.
            var monthlyRevenue = await db.ProcedurePayments
                .Include(pp => pp.ServiceMenu)
                .Include(pp => pp.PatientProcedure)
                .Where(pp =>
                    pp.Status.ToLower() == "completed" &&
                    pp.PatientProcedure != null &&
                    pp.PatientProcedure.CreatedAt >= monthStart &&
                    pp.PatientProcedure.CreatedAt < monthEnd)
                .SumAsync(pp => pp.ServiceMenu != null ? pp.ServiceMenu.Price : 0.0);

            // ── 3. Top 5 procedures this month ────────────────────────────────
            var procedureGroups = await db.PatientProcedures
                .Include(p => p.ServiceMenu)
                .Where(p =>
                    p.ServiceMenu != null &&
                    p.CreatedAt >= monthStart &&
                    p.CreatedAt < monthEnd)
                .GroupBy(p => p.ServiceMenu!.Name)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToListAsync();

            int maxCount = procedureGroups.Count > 0 ? procedureGroups[0].Count : 1;

            var topProcedures = procedureGroups.Select(g => new ProcedureStatDto
            {
                ProcedureName = g.Name,
                Count = g.Count,
                BarPercent = (int)Math.Round((double)g.Count / maxCount * 100)
            }).ToList();

            // ── 4. Low stock medicines ────────────────────────────────────────
            var lowStock = await db.Medicines
                .Where(m => m.Stock < LowStockThreshold)
                .OrderBy(m => m.Stock)
                .Select(m => new LowStockMedicineDto
                {
                    Name = m.Name,
                    Stock = m.Stock,
                    Threshold = LowStockThreshold
                })
                .ToListAsync();

            return new DashboardDto
            {
                TotalPatients = totalPatients,
                MonthlyRevenue = monthlyRevenue,
                TopProcedures = topProcedures,
                LowStockMedicines = lowStock
            };
        }
    }
}