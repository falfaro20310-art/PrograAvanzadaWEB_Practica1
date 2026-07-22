using System.ComponentModel.DataAnnotations;

namespace VitalApp_API.Models
{
    public class RecoverPasswordRequestModel
    {
        [Required]
        public string Email { get; set; } = string.Empty;
    }
}
