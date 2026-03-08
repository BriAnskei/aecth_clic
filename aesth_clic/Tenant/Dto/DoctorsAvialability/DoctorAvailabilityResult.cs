namespace aesth_clic.Tenant.DTO
{
    public class DoctorAvailabilityDto
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string AvailabilityStatus { get; set; } = string.Empty; // available | busy
    }
}