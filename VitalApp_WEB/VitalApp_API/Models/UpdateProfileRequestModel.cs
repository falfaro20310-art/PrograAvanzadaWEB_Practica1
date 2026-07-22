using System.ComponentModel.DataAnnotations;

namespace VitalApp_API.Models
{
    public class UpdateProfileRequestModel
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public string Gender { get; set; } = string.Empty;
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
    }
}
