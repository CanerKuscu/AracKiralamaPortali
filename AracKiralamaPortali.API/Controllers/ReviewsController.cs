using System.Security.Claims;
using AracKiralamaPortali.API.DTOs;
using AracKiralamaPortali.API.Models;
using AracKiralamaPortali.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AracKiralamaPortali.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly IRepository<Review> _reviewRepository;
        private readonly IRepository<Reservation> _reservationRepository;

        public ReviewsController(
            IRepository<Review> reviewRepository,
            IRepository<Reservation> reservationRepository)
        {
            _reviewRepository = reviewRepository;
            _reservationRepository = reservationRepository;
        }

        private ReviewDto MapToDto(Review r) => new ReviewDto
        {
            Id = r.Id,
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt,
            VehicleId = r.VehicleId,
            VehiclePlate = r.Vehicle.Plate,
            BrandName = r.Vehicle.Brand.Name,
            AppUserId = r.AppUserId,
            UserFullName = r.AppUser.FullName,
            ReservationId = r.ReservationId
        };

        [HttpGet("vehicle/{vehicleId}")]
        public async Task<IActionResult> GetByVehicle(int vehicleId)
        {
            var reviews = await _reviewRepository.GetQueryable()
                .Include(r => r.Vehicle).ThenInclude(v => v.Brand)
                .Include(r => r.AppUser)
                .Where(r => r.VehicleId == vehicleId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var summary = new VehicleReviewSummaryDto
            {
                VehicleId = vehicleId,
                AverageRating = reviews.Count > 0 ? reviews.Average(r => r.Rating) : 0,
                TotalReviews = reviews.Count,
                Reviews = reviews.Select(MapToDto).ToList()
            };

            return Ok(summary);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var reviews = await _reviewRepository.GetQueryable()
                .Include(r => r.Vehicle).ThenInclude(v => v.Brand)
                .Include(r => r.AppUser)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(reviews.Select(MapToDto));
        }

        [Authorize]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyReviews()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reviews = await _reviewRepository.GetQueryable()
                .Include(r => r.Vehicle).ThenInclude(v => v.Brand)
                .Include(r => r.AppUser)
                .Where(r => r.AppUserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(reviews.Select(MapToDto));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReviewCreateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var reservation = await _reservationRepository.GetByIdAsync(dto.ReservationId);
            if (reservation == null)
                return NotFound(new { message = "Rezervasyon bulunamadý." });

            if (reservation.AppUserId != userId)
                return Forbid();

            if (reservation.Status != "Completed" && reservation.Status != "Confirmed")
                return BadRequest(new { message = "Yalnýzca tamamlanan veya onaylanan rezervasyonlar için yorum yapabilirsiniz." });

            if (reservation.VehicleId != dto.VehicleId)
                return BadRequest(new { message = "Araç bilgisi rezervasyon ile eþleþmiyor." });

            var alreadyReviewed = await _reviewRepository.AnyAsync(r =>
                r.ReservationId == dto.ReservationId && r.AppUserId == userId);
            if (alreadyReviewed)
                return BadRequest(new { message = "Bu rezervasyon için zaten yorum yaptýnýz." });

            var review = new Review
            {
                Rating = dto.Rating,
                Comment = dto.Comment,
                VehicleId = dto.VehicleId,
                AppUserId = userId!,
                ReservationId = dto.ReservationId
            };

            await _reviewRepository.AddAsync(review);
            await _reviewRepository.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByVehicle), new { vehicleId = review.VehicleId },
                new { review.Id, review.Rating, review.Comment });
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (review.AppUserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            _reviewRepository.Delete(review);
            await _reviewRepository.SaveChangesAsync();
            return Ok(new { message = "Yorum baþarýyla silindi." });
        }
    }
}
