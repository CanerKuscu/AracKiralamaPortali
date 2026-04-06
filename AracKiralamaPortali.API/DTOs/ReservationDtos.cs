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
        public string? CurrentLocationText { get; set; }
        public double? CurrentLatitude { get; set; }
        public double? CurrentLongitude { get; set; }
        public DateTime? LocationUpdatedAt { get; set; }
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
        [Required(ErrorMessage = "Baþlangýç tarihi zorunludur.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Bitiþ tarihi zorunludur.")]
        public DateTime EndDate { get; set; }

        [MaxLength(500, ErrorMessage = "Not alaný en fazla 500 karakter olabilir.")]
        public string? Notes { get; set; }

        [MaxLength(200, ErrorMessage = "Teslim alma konumu en fazla 200 karakter olabilir.")]
        public string? PickupLocation { get; set; }

        [MaxLength(200, ErrorMessage = "Teslim býrakma konumu en fazla 200 karakter olabilir.")]
        public string? DropoffLocation { get; set; }

        public int VehicleId { get; set; }
        public List<int>? AdditionalServiceIds { get; set; }
    }

    public class ReservationUpdateDto
    {
        [Required(ErrorMessage = "Baþlangýç tarihi zorunludur.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Bitiþ tarihi zorunludur.")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Durum alaný zorunludur.")]
        [MaxLength(50, ErrorMessage = "Durum alaný en fazla 50 karakter olabilir.")]
        public string Status { get; set; } = null!;

        [MaxLength(500, ErrorMessage = "Not alaný en fazla 500 karakter olabilir.")]
        public string? Notes { get; set; }
    }
}
