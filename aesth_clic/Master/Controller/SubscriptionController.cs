using aesth_clic.Master.Services;
using aesth_clic.ViewModels.SuperAdmin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace aesth_clic.Master.Controller
{
    internal sealed class SubscriptionController(SubscriptionService subscriptionService)
    {
        private readonly SubscriptionService _subscriptionService =
            subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));

        /// <summary>
        /// Returns all subscriptions enriched with clinic and admin user details.
        /// </summary>
        public Task<List<SubscriptionDbRow>> GetAllSubcriptionsAsync()
            => _subscriptionService.GetAllSubcriptionsAsync();

        /// <summary>
        /// Marks the current month as paid and extends the subscription by 1 month.
        /// </summary>
        public Task MarkCurrentMonthAsPaidAsync(int subscriptionId)
            => _subscriptionService.MarkCurrentMonthAsPaidAsync(subscriptionId);
    }
}