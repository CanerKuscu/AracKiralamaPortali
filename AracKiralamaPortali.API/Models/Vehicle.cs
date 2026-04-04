using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AracKiralamaPortali.API.Models
{
    public class Vehicle
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Model { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Plate { get; set; } = null!;

        public int Year { get; set; }

        [Required, MaxLength(50)]
        public string Color { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DailyPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? WeeklyPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MonthlyPrice { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        [Required, MaxLength(30)]
        public string VehicleStatus { get; set; } = "Available";

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required, MaxLength(50)]
        public string FuelType { get; set; } = null!;

        [Required, MaxLength(50)]
        public string TransmissionType { get; set; } = null!;

        public int Mileage { get; set; }
        public int PassengerCapacity { get; set; } = 5;
        public int LuggageCapacity { get; set; } = 2;

        public DateTime? InsuranceExpiryDate { get; set; }
        public DateTime? InspectionExpiryDate { get; set; }
        public bool HasAccidentHistory { get; set; } = false;

        public int BrandId { get; set; }
        [JsonIgnore]
        public Brand Brand { get; set; } = null!;

        public string? OwnerId { get; set; }
        [JsonIgnore]
        public AppUser? Owner { get; set; }

        [JsonIgnore]
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        [JsonIgnore]
        public ICollection<Maintenance> Maintenances { get; set; } = new List<Maintenance>();
        [JsonIgnore]
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        [JsonIgnore]
        public ICollection<VehicleImage> Images { get; set; } = new List<VehicleImage>();
    }
}
