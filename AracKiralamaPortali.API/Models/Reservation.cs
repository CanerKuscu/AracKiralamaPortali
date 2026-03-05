using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AracKiralamaPortali.API.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DepositAmount { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; } = "Pending";

        [MaxLength(200)]
        public string? PickupLocation { get; set; }

        [MaxLength(200)]
        public string? DropoffLocation { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public string AppUserId { get; set; } = null!;
        public AppUser AppUser { get; set; } = null!;

        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; } = null!;

        public Payment? Payment { get; set; }
        public ICollection<ReservationService> ReservationServices { get; set; } = new List<ReservationService>();
    }
}
