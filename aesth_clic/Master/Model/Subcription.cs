using System;

namespace aesth_clic.Master.Model
{
    public class Subscription
    {
        public int Id { get; set; }

        public int ClientId { get; set; }


        public string Tier { get; set; } = string.Empty;

        public decimal MonthlyAmount { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

    }
}