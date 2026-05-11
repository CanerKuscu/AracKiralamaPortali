using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AracKiralamaPortali.API.Models
{
    public class VehicleQuestion
    {
        public int Id { get; set; }

        public int VehicleId { get; set; }
        [JsonIgnore]
        public Vehicle Vehicle { get; set; } = null!;

        public string UserId { get; set; } = null!; // The user who asks the question
        [JsonIgnore]
        public AppUser User { get; set; } = null!;

        [Required, MaxLength(500)]
        public string Question { get; set; } = null!;

        [MaxLength(1000)]
        public string? Answer { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? AnsweredAt { get; set; }
        public bool IsAnswered { get; set; } = false;
    }
}
