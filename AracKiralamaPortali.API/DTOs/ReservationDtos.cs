using System.ComponentModel.DataAnnotations;

namespace AracKiralamaPortali.API.DTOs
{
    public class ReservationDto
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal DepositAmount { get; set; }
        public string Status { get; set; } = null!;
        public string? PickupLocation { get; set; }
        public string? DropoffLocation { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Notes { get; set; }
        public string AppUserId { get; set; } = null!;
        public string UserFullName { get; set; } = null!;
        public int VehicleId { get; set; }
        public string VehiclePlate { get; set; } = null!;
        public string BrandName { get; set; } = null!;
        public List<string> AdditionalServices { get; set; } = new();
    }

    public class ReservationCreateDto
    {
        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [MaxLength(200)]
        public string? PickupLocation { get; set; }

        [MaxLength(200)]
        public string? DropoffLocation { get; set; }

        public int VehicleId { get; set; }
        public List<int>? AdditionalServiceIds { get; set; }
    }

    public class ReservationUpdateDto
    {
        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; } = null!;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
