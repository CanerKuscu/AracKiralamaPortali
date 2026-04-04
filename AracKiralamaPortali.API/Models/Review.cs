using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AracKiralamaPortali.API.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int VehicleId { get; set; }
        [JsonIgnore]
        public Vehicle Vehicle { get; set; } = null!;

        public string AppUserId { get; set; } = null!;
        [JsonIgnore]
        public AppUser AppUser { get; set; } = null!;

        public int ReservationId { get; set; }
        [JsonIgnore]
        public Reservation Reservation { get; set; } = null!;
    }
}
