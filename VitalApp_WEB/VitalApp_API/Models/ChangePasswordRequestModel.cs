using System.ComponentModel.DataAnnotations;

namespace VitalApp_API.Models
{
    public class ChangePasswordRequestModel
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
