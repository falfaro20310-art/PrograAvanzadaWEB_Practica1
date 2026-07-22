namespace VitalApp_API.Models
{
    public class ProfileResponseModel
    {
        public int UserId { get; set; }
        public int ProfileId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string IdCard { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public string Gender { get; set; } = string.Empty;
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
    }
}
