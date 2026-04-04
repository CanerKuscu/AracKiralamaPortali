using System.ComponentModel.DataAnnotations;

namespace AracKiralamaPortali.API.DTOs
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public int VehicleId { get; set; }
        public string VehiclePlate { get; set; } = null!;
        public string BrandName { get; set; } = null!;
        public string AppUserId { get; set; } = null!;
        public string UserFullName { get; set; } = null!;
        public int ReservationId { get; set; }
    }

    public class ReviewCreateDto
    {
        [Required, Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public int VehicleId { get; set; }
        public int ReservationId { get; set; }
    }

    public class VehicleReviewSummaryDto
    {
        public int VehicleId { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public List<ReviewDto> Reviews { get; set; } = new();
    }
}
