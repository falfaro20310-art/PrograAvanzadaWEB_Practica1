using System.ComponentModel.DataAnnotations;

namespace VitalApp_API.Models
{
    public class UpdateMeasureRequestModel
    {
        [Required]
        public int MeasureId { get; set; }
        [Required]
        public decimal Value { get; set; }
        public decimal SecondaryValue { get; set; }
        [Required]
        public DateTime MeasureDate { get; set; }
        public string? Notes { get; set; }
    }
}
