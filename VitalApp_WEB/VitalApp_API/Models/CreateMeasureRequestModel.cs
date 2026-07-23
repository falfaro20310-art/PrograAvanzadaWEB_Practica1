using System.ComponentModel.DataAnnotations;

namespace VitalApp_API.Models
{
    public class CreateMeasureRequestModel
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public int IndicatorTypeId { get; set; }
        [Required]
        public decimal Value { get; set; }
        public decimal SecondaryValue { get; set; }
        [Required]
        public DateTime MeasureDate { get; set; }
        public string? Notes { get; set; }
    }
}
