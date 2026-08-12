using System.ComponentModel.DataAnnotations;

namespace VitalApp_API.Models
{
    public class ChangeRoleRequestModel
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public int RoleId { get; set; }
    }
}
