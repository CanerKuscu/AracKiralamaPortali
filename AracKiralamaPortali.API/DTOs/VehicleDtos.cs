using System.ComponentModel.DataAnnotations;

namespace AracKiralamaPortali.API.DTOs
{
    public class VehicleDto
    {
        public int Id { get; set; }
        public string Model { get; set; } = null!;
        public string Plate { get; set; } = null!;
        public int Year { get; set; }
        public string Color { get; set; } = null!;
        public decimal DailyPrice { get; set; }
        public decimal? WeeklyPrice { get; set; }
        public decimal? MonthlyPrice { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string VehicleStatus { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string FuelType { get; set; } = null!;
        public string TransmissionType { get; set; } = null!;
        public int Mileage { get; set; }
        public int PassengerCapacity { get; set; }
        public int LuggageCapacity { get; set; }
        public int BrandId { get; set; }
        public string BrandName { get; set; } = null!;
        public string? City { get; set; }
        public string? District { get; set; }
        public DateTime? InsuranceExpiryDate { get; set; }
        public DateTime? InspectionExpiryDate { get; set; }
        public bool HasAccidentHistory { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public string? OwnerId { get; set; }
    }

    public class VehicleCreateDto
    {
        [Required, MaxLength(100)]
        public string Model { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Plate { get; set; } = null!;

        public int Year { get; set; }

        [Required, MaxLength(50)]
        public string Color { get; set; } = null!;

        public decimal DailyPrice { get; set; }
        public decimal? WeeklyPrice { get; set; }
        public decimal? MonthlyPrice { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        [Required, MaxLength(50)]
        public string FuelType { get; set; } = null!;

        [Required, MaxLength(50)]
        public string TransmissionType { get; set; } = null!;

        public int Mileage { get; set; }
        public int PassengerCapacity { get; set; } = 5;
        public int LuggageCapacity { get; set; } = 2;
        public string? City { get; set; }
        public string? District { get; set; }
        public int BrandId { get; set; }
        public string? BrandName { get; set; }
        public DateTime? InsuranceExpiryDate { get; set; }
        public DateTime? InspectionExpiryDate { get; set; }
        public bool HasAccidentHistory { get; set; }
    }

    public class VehicleUpdateDto
    {
        [Required, MaxLength(100)]
        public string Model { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Plate { get; set; } = null!;

        public int Year { get; set; }

        [Required, MaxLength(50)]
        public string Color { get; set; } = null!;

        public decimal DailyPrice { get; set; }
        public decimal? WeeklyPrice { get; set; }
        public decimal? MonthlyPrice { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        [Required, MaxLength(30)]
        public string VehicleStatus { get; set; } = null!;

        public bool IsActive { get; set; }

        [Required, MaxLength(50)]
        public string FuelType { get; set; } = null!;

        [Required, MaxLength(50)]
        public string TransmissionType { get; set; } = null!;

        public int Mileage { get; set; }
        public int PassengerCapacity { get; set; }
        public int LuggageCapacity { get; set; }
        public int BrandId { get; set; }
        public string? BrandName { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public DateTime? InsuranceExpiryDate { get; set; }
        public DateTime? InspectionExpiryDate { get; set; }
        public bool HasAccidentHistory { get; set; }
    }

    public class VehicleQuestionDto
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Question { get; set; } = null!;
        public string? Answer { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? AnsweredAt { get; set; }
        public bool IsAnswered { get; set; }
    }

    public class VehicleQuestionCreateDto
    {
        public int VehicleId { get; set; }
        [Required, MaxLength(500)]
        public string Question { get; set; } = null!;
    }

    public class VehicleQuestionAnswerDto
    {
        [Required, MaxLength(1000)]
        public string Answer { get; set; } = null!;
    }
}

