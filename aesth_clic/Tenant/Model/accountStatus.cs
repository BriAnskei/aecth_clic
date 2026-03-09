namespace aesth_clic.Tenant.Model
{
    public class AccountStatus
    {
        public int Id { get; set; } = 0;

        public int AccountId { get; set; } = 0;

        public string Status { get; set; } = "active"; // default active (active, deactivated)

        public User? User { get; set; }
    }
}
