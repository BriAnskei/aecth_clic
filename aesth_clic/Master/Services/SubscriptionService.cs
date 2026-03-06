using aesth_clic.Context;
using aesth_clic.Master.Controller;
using aesth_clic.ViewModels.SuperAdmin;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace aesth_clic.Master.Services
{
    internal sealed class SubscriptionService(
        MasterDbContext masterDb,
        AdminUserController adminUserController)
    {
        private readonly MasterDbContext _masterDb =
            masterDb ?? throw new ArgumentNullException(nameof(masterDb));

        private readonly AdminUserController _adminUserController =
            adminUserController ?? throw new ArgumentNullException(nameof(adminUserController));

        // ─────────────────────────────────────────────────────────────
        // GET ALL SUBSCRIPTIONS
        // Join Client + Subscription then enrich from Tenant DB
        // ─────────────────────────────────────────────────────────────
        public async Task<List<SubscriptionDbRow>> GetAllSubcriptionsAsync()
        {
            try
            {
                var rawRows = await (
                    from s in _masterDb.Subscription
                    join c in _masterDb.Clients on s.ClientId equals c.Id
                    orderby s.EndDate ascending
                    select new
                    {
                        SubscriptionId = s.Id,
                        ClientId = c.Id,
                        c.ClinicName,
                        c.Tier,
                        s.MonthlyAmount,
                        s.StartDate,
                        s.EndDate
                    }
                ).ToListAsync();

                var adminDetails = await _adminUserController.GetAllAdminClinicsAsync();

                var detailsById = adminDetails
                    .GroupBy(d => d.ClientId)
                    .ToDictionary(g => g.Key, g => g.First());

                return rawRows.Select(r =>
                {
                    detailsById.TryGetValue(r.ClientId, out var detail);

                    return new SubscriptionDbRow
                    {
                        SubscriptionId = r.SubscriptionId,
                        UserId = r.ClientId,
                        FullName = detail?.FullName ?? r.ClinicName,
                        Email = detail?.Email ?? string.Empty,
                        ClinicName = r.ClinicName,
                        Tier = r.Tier,
                        MonthlyAmount = r.MonthlyAmount,
                        StartDate = r.StartDate,
                        EndDate = r.EndDate
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                // Include method name and message for debugging
                Console.WriteLine($"[ERROR] GetAllSubcriptionsAsync failed: {ex}");
                return new List<SubscriptionDbRow>(); // Return empty list on failure
            }
        }

        // ─────────────────────────────────────────────────────────────
        // MARK CURRENT MONTH AS PAID
        // StartDate becomes previous EndDate
        // EndDate += 1 month
        // ─────────────────────────────────────────────────────────────
        public async Task MarkCurrentMonthAsPaidAsync(int subscriptionId)
        {
            try
            {
                var subscription = await _masterDb.Subscription
                    .FirstOrDefaultAsync(s => s.Id == subscriptionId);

                if (subscription is null)
                    throw new InvalidOperationException($"Subscription {subscriptionId} not found.");

                var previousEndDate = subscription.EndDate;

                subscription.StartDate = previousEndDate;
                subscription.EndDate = previousEndDate.AddMonths(1);

                await _masterDb.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Include subscriptionId and method name for debugging
                Console.WriteLine($"[ERROR] MarkCurrentMonthAsPaidAsync failed for SubscriptionId {subscriptionId}: {ex}");
            }
        }
    }
}