namespace aesth_clic.Tenant.Model
{
    public class ServiceMenu
    {
        public int Id { get; set; } = 0;
        public int AddedBy { get; set; } = 0; // doctors
        public string Name { get; set; } = string.Empty;

        public double Price { get; set; } = 0.0;

        public User? User { get; set; }
    }
}
