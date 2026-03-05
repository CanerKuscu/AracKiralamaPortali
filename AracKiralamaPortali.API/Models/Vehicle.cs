using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        public int BrandId { get; set; }
        public Brand Brand { get; set; } = null!;

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public ICollection<Maintenance> Maintenances { get; set; } = new List<Maintenance>();
    }
}
