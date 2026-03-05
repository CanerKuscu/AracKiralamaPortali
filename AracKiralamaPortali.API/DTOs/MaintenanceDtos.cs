using System.ComponentModel.DataAnnotations;

namespace AracKiralamaPortali.API.DTOs
{
    public class MaintenanceDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime Date { get; set; }
        public DateTime? NextDate { get; set; }
        public decimal Cost { get; set; }
        public int MileageAtService { get; set; }
        public string Status { get; set; } = null!;
        public int VehicleId { get; set; }
        public string VehiclePlate { get; set; } = null!;
        public string BrandName { get; set; } = null!;
    }

    public class MaintenanceCreateDto
    {
        [Required, MaxLength(100)]
        public string Type { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime Date { get; set; }
        public DateTime? NextDate { get; set; }
        public decimal Cost { get; set; }
        public int MileageAtService { get; set; }
        public int VehicleId { get; set; }
    }

    public class MaintenanceUpdateDto
    {
        [Required, MaxLength(100)]
        public string Type { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime Date { get; set; }
        public DateTime? NextDate { get; set; }
        public decimal Cost { get; set; }
        public int MileageAtService { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; } = null!;
    }
}
