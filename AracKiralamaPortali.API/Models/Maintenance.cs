using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AracKiralamaPortali.API.Models
{
    public class Maintenance
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Type { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime Date { get; set; }
        public DateTime? NextDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; }

        public int MileageAtService { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; } = "Planned";

        public int VehicleId { get; set; }
        [JsonIgnore]
        public Vehicle Vehicle { get; set; } = null!;
    }
}
