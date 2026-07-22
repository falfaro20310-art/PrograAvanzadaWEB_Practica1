using System.ComponentModel.DataAnnotations;

namespace VitalApp_API.Models
{
    public class LoginRequestModel
    {
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
