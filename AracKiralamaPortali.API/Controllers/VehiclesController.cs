using AracKiralamaPortali.API.DTOs;
using AracKiralamaPortali.API.Models;
using AracKiralamaPortali.API.Repositories;
using AracKiralamaPortali.API.Data;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace AracKiralamaPortali.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehiclesController(IVehicleRepository vehicleRepository, IBrandRepository brandRepository, IMapper mapper, AppDbContext context) : ControllerBase
    {
        private IQueryable<Vehicle> GetPublicVehicleQuery()
        {
            return vehicleRepository.GetQueryable()
                .Where(v => v.OwnerId == null || (v.Owner != null && !v.Owner.IsDeleted && v.Owner.IsActive));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var vehicles = await GetPublicVehicleQuery()
                .Include(v => v.Brand).Include(v => v.Reviews).Include(v => v.Images)
                .Include(v => v.Reservations)
                .ToListAsync();
            
            var dtos = vehicles.Select(v => {
                var dto = mapper.Map<VehicleDto>(v);
                
                // Eðer þu anda kirada ise durumu güncelle
                var currentRental = v.Reservations.FirstOrDefault(r =>
                    r.Status == "Confirmed" && r.StartDate <= DateTime.Now && r.EndDate >= DateTime.Now);
                
                if (currentRental != null)
                {
                    dto.VehicleStatus = "Rented";
                }
                
                return dto;
            });
            
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var vehicle = await GetPublicVehicleQuery()
                .Include(v => v.Brand).Include(v => v.Reviews).Include(v => v.Images)
                .Include(v => v.Reservations)
                .FirstOrDefaultAsync(v => v.Id == id);
            if (vehicle == null) return NotFound();
            
            var dto = mapper.Map<VehicleDto>(vehicle);
            
            // Eðer þu anda kirada ise durumu güncelle
            var currentRental = vehicle.Reservations.FirstOrDefault(r =>
                r.Status == "Confirmed" && r.StartDate <= DateTime.Now && r.EndDate >= DateTime.Now);
            
            if (currentRental != null)
            {
                dto.VehicleStatus = "Rented";
            }
            
            return Ok(dto);
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable()
        {
            var vehicles = await GetPublicVehicleQuery()
                .Include(v => v.Brand).Include(v => v.Reviews).Include(v => v.Images)
                .Include(v => v.Reservations)
                .ToListAsync();

            var availableVehicles = vehicles.Where(v => v.IsActive && 
                (v.VehicleStatus != null && (v.VehicleStatus.ToLower() == "available" || v.VehicleStatus.ToLower() == "musait" || v.VehicleStatus.ToLower() == "müsait" || v.VehicleStatus.ToLower() == "rented"))).ToList()
                .Where(v => !v.Reservations.Any(r => r.Status != "Cancelled" && r.Status != "Completed" && 
                    r.StartDate <= DateTime.Now && r.EndDate >= DateTime.Now))
                .ToList();
            
            return Ok(availableVehicles.Select(v => {
                var dto = mapper.Map<VehicleDto>(v);
                var currentRental = v.Reservations.FirstOrDefault(r =>
                    r.Status == "Confirmed" && r.StartDate <= DateTime.Now && r.EndDate >= DateTime.Now);
                if (currentRental != null) dto.VehicleStatus = "Rented";
                return dto;
            }));
        }

        [HttpGet("brand/{brandId}")]
        public async Task<IActionResult> GetByBrand(int brandId)
        {
            var vehicles = await GetPublicVehicleQuery()
                .Include(v => v.Brand).Include(v => v.Reviews).Include(v => v.Images)
                .Include(v => v.Reservations)
                .Where(v => v.BrandId == brandId).ToListAsync();
            
            var dtos = vehicles.Select(v => {
                var dto = mapper.Map<VehicleDto>(v);
                
                // Eðer þu anda kirada ise durumu güncelle
                var currentRental = v.Reservations.FirstOrDefault(r =>
                    r.Status == "Confirmed" && r.StartDate <= DateTime.Now && r.EndDate >= DateTime.Now);
                
                if (currentRental != null)
                {
                    dto.VehicleStatus = "Rented";
                }
                
                return dto;
            });
            
            return Ok(dtos);
        }

        [HttpGet("filter")]
        public async Task<IActionResult> Filter([FromQuery] string? fuelType, [FromQuery] string? transmissionType, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice, [FromQuery] int? brandId, [FromQuery] int? minPassenger)
        {
            var query = GetPublicVehicleQuery()
                .Include(v => v.Brand).Include(v => v.Reviews).Include(v => v.Images)
                .Include(v => v.Reservations)
                .Where(v => v.IsActive);

            if (!string.IsNullOrEmpty(fuelType)) query = query.Where(v => v.FuelType == fuelType);
            if (!string.IsNullOrEmpty(transmissionType)) query = query.Where(v => v.TransmissionType == transmissionType);
            if (minPrice.HasValue) query = query.Where(v => v.DailyPrice >= minPrice);
            if (maxPrice.HasValue) query = query.Where(v => v.DailyPrice <= maxPrice);
            if (brandId.HasValue) query = query.Where(v => v.BrandId == brandId);
            if (minPassenger.HasValue) query = query.Where(v => v.PassengerCapacity >= minPassenger);

            var vehicles = await query.ToListAsync();
            
            // Þu anda kirada olmayan araçlarý filtrele
            var availableVehicles = vehicles.Where(v => 
                (v.VehicleStatus != null && (v.VehicleStatus.ToLower() == "available" || v.VehicleStatus.ToLower() == "musait" || v.VehicleStatus.ToLower() == "müsait" || v.VehicleStatus.ToLower() == "rented" || v.VehicleStatus == "msait")) &&
                !v.Reservations.Any(r => r.Status != "Cancelled" && r.Status != "Completed" && 
                    r.StartDate <= DateTime.Now && r.EndDate >= DateTime.Now))
                .ToList();

            return Ok(availableVehicles.Select(v => {
                var dto = mapper.Map<VehicleDto>(v);
                var currentRental = v.Reservations.FirstOrDefault(r =>
                    r.Status == "Confirmed" && r.StartDate <= DateTime.Now && r.EndDate >= DateTime.Now);
                if (currentRental != null) dto.VehicleStatus = "Rented";
                return dto;
            }));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VehicleCreateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            int resolvedBrandId = dto.BrandId;
            if (!string.IsNullOrWhiteSpace(dto.BrandName))
            {
                var brandName = dto.BrandName.Trim();
                var existingBrand = await brandRepository.GetQueryable()
                    .FirstOrDefaultAsync(b => b.Name == brandName);
                
                if (existingBrand != null)
                {
                    resolvedBrandId = existingBrand.Id;
                }
                else
                {
                    var newBrand = new Brand { Name = dto.BrandName.Trim(), IsActive = true };
                    await brandRepository.AddAsync(newBrand);
                    await brandRepository.SaveChangesAsync();
                    resolvedBrandId = newBrand.Id;
                }
            }

            var vehicle = mapper.Map<Vehicle>(dto);
            vehicle.BrandId = resolvedBrandId;
            vehicle.OwnerId = userId;
            await vehicleRepository.AddAsync(vehicle);
            await vehicleRepository.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = vehicle.Id }, new { id = vehicle.Id, message = "Araç baþarýyla oluþturuldu." });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] VehicleUpdateDto dto)
        {
            var vehicle = await vehicleRepository.GetByIdAsync(id);
            if (vehicle == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            
            if (vehicle.OwnerId != userId && userRole != "Admin")
                return Forbid();

            int resolvedBrandId = dto.BrandId;
            if (!string.IsNullOrWhiteSpace(dto.BrandName))
            {
                var brandName = dto.BrandName.Trim();
                var existingBrand = await brandRepository.GetQueryable()
                    .FirstOrDefaultAsync(b => b.Name == brandName);
                
                if (existingBrand != null)
                {
                    resolvedBrandId = existingBrand.Id;
                }
                else
                {
                    var newBrand = new Brand { Name = dto.BrandName.Trim(), IsActive = true };
                    await brandRepository.AddAsync(newBrand);
                    await brandRepository.SaveChangesAsync();
                    resolvedBrandId = newBrand.Id;
                }
            }

            mapper.Map(dto, vehicle);
            vehicle.BrandId = resolvedBrandId;

            vehicleRepository.Update(vehicle);
            await vehicleRepository.SaveChangesAsync();
            return Ok(new { message = "Araç baþarýyla güncellendi." });
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var vehicle = await vehicleRepository.GetByIdAsync(id);
            if (vehicle == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            
            if (vehicle.OwnerId != userId && userRole != "Admin")
                return Forbid();

            vehicleRepository.Delete(vehicle);
            await vehicleRepository.SaveChangesAsync();
            return Ok(new { message = "Araç baþarýyla silindi." });
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("fleet")]
        public async Task<IActionResult> GetFleet()
        {
            var vehicles = await vehicleRepository.GetQueryable()
                .Where(v => v.IsActive)
                .Where(v => v.OwnerId == null || (v.Owner != null && !v.Owner.IsDeleted && v.Owner.IsActive))
                .Include(v => v.Brand)
                .Include(v => v.Owner)
                .Include(v => v.Reservations.Where(r => r.Status != "Cancelled"))
                    .ThenInclude(r => r.AppUser)
                .ToListAsync();

            vehicles = vehicles
                .GroupBy(v => v.Plate.Trim().ToLower())
                .Select(g => g.OrderByDescending(v => v.Id).First())
                .ToList();

            var result = vehicles.Select(v => {
                var activeRes = v.Reservations.FirstOrDefault(r => r.Status == "Confirmed" && r.StartDate <= DateTime.Now && r.EndDate >= DateTime.Now);
                return new {
                    v.Id, v.Plate, v.Model, BrandName = v.Brand.Name, v.VehicleStatus,
                    CurrentUser = activeRes?.AppUser.FullName,
                    ReturnDate = activeRes?.EndDate,
                    CurrentLocation = activeRes?.CurrentLocationText ?? activeRes?.PickupLocation,
                    CurrentLatitude = activeRes?.CurrentLatitude,
                    CurrentLongitude = activeRes?.CurrentLongitude,
                    LocationUpdatedAt = activeRes?.LocationUpdatedAt
                };
            });
            return Ok(result);
        }

        // --- Reviews ---
        [HttpGet("{id}/reviews")]
        public async Task<IActionResult> GetReviews(int id)
        {
            var reviews = await context.Reviews
                .Include(r => r.AppUser)
                .Include(r => r.Vehicle)
                    .ThenInclude(v => v.Brand)
                .Where(r => r.VehicleId == id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var dtos = reviews.Select(r => new ReviewDto
            {
                Id = r.Id,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                AppUserId = r.AppUserId,
                UserFullName = r.AppUser?.FullName ?? "Anonim",
                VehicleId = r.VehicleId,
                ReservationId = r.ReservationId,
                VehiclePlate = r.Vehicle?.Plate ?? string.Empty,
                BrandName = r.Vehicle?.Brand?.Name ?? string.Empty
            });

            return Ok(dtos);
        }

        [Authorize]
        [HttpGet("{id}/can-review")]
        public async Task<IActionResult> CanReview(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var hasRented = await context.Reservations
                .AnyAsync(r => r.VehicleId == id && r.AppUserId == userId && (r.Status == "Completed" || r.Status == "Confirmed"));

            return Ok(new { canReview = hasRented });
        }

        [Authorize]
        [HttpPost("{id}/reviews")]
        public async Task<IActionResult> AddReview(int id, [FromBody] ReviewCreateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Kullanýcýnýn bu aracý daha önce kiralayýp tamamladýðýný (veya kullanýmda olduðunu) kontrol edelim
            var hasRented = await context.Reservations
                .AnyAsync(r => r.VehicleId == id && r.AppUserId == userId && (r.Status == "Completed" || r.Status == "Confirmed"));
                
            if (!hasRented)
            {
                return BadRequest(new { message = "Sadece kiraladýðýnýz araçlara yorum yapabilirsiniz." });
            }

            // Gerekli ise son kiralama id'sini bul
            var lastReservation = await context.Reservations
                .Where(r => r.VehicleId == id && r.AppUserId == userId && (r.Status == "Completed" || r.Status == "Confirmed"))
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            var review = new Review
            {
                VehicleId = id,
                AppUserId = userId!,
                Rating = dto.Rating,
                Comment = dto.Comment,
                ReservationId = dto.ReservationId == 0 ? (lastReservation?.Id ?? 0) : dto.ReservationId,
                CreatedAt = DateTime.Now
            };

            context.Reviews.Add(review);
            await context.SaveChangesAsync();

            return Ok(new { message = "Yorumunuz baþarýyla eklendi." });
        }

        // --- Questions ---
        [HttpGet("{id}/questions")]
        public async Task<IActionResult> GetQuestions(int id)
        {
            var questions = await context.VehicleQuestions
                .Include(q => q.User)
                .Where(q => q.VehicleId == id)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            var dtos = questions.Select(q => new VehicleQuestionDto
            {
                Id = q.Id,
                VehicleId = q.VehicleId,
                UserId = q.UserId,
                UserName = q.User?.FullName ?? "Anonim",
                Question = q.Question,
                Answer = q.Answer,
                CreatedAt = q.CreatedAt,
                AnsweredAt = q.AnsweredAt,
                IsAnswered = q.IsAnswered
            });

            return Ok(dtos);
        }

        [Authorize]
        [HttpPost("{id}/questions")]
        public async Task<IActionResult> AddQuestion(int id, [FromBody] VehicleQuestionCreateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var vehicle = await context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound(new { message = "Araç bulunamadý." });

            if (vehicle.OwnerId == userId)
            {
                return BadRequest(new { message = "Araç sahibi kendi aracýna soru soramaz." });
            }
            
            var question = new VehicleQuestion
            {
                VehicleId = id,
                UserId = userId!,
                Question = dto.Question,
                CreatedAt = DateTime.Now,
                IsAnswered = false
            };

            context.VehicleQuestions.Add(question);
            await context.SaveChangesAsync();

            return Ok(new { message = "Sorunuz araç sahibine iletildi." });
        }

        [Authorize]
        [HttpPost("{id}/questions/{questionId}/answer")]
        public async Task<IActionResult> AnswerQuestion(int id, int questionId, [FromBody] VehicleQuestionAnswerDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            
            var vehicle = await context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound("Araç bulunamadý.");

            if (vehicle.OwnerId != userId)
                return Forbid();

            var question = await context.VehicleQuestions.FindAsync(questionId);
            if (question == null || question.VehicleId != id) return NotFound("Soru bulunamadý.");

            question.Answer = dto.Answer;
            question.IsAnswered = true;
            question.AnsweredAt = DateTime.Now;

            await context.SaveChangesAsync();
            return Ok(new { message = "Soru baþarýyla cevaplandý." });
        }

        [Authorize]
        [HttpGet("owner-notifications")]
        public async Task<IActionResult> GetOwnerNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var questionNotifications = await context.VehicleQuestions
                .Include(q => q.Vehicle)
                    .ThenInclude(v => v.Brand)
                .Include(q => q.User)
                .Where(q => q.Vehicle.OwnerId == userId && !q.IsAnswered)
                .Select(q => new OwnerNotificationDto
                {
                    Type = "Question",
                    VehicleId = q.VehicleId,
                    VehiclePlate = q.Vehicle.Plate,
                    BrandName = q.Vehicle.Brand != null ? q.Vehicle.Brand.Name : string.Empty,
                    UserFullName = q.User != null ? q.User.FullName : "Anonim",
                    Message = q.Question,
                    IsAnswered = q.IsAnswered,
                    CreatedAt = q.CreatedAt
                })
                .ToListAsync();

            var reviewNotifications = await context.Reviews
                .Include(r => r.Vehicle)
                    .ThenInclude(v => v.Brand)
                .Include(r => r.AppUser)
                .Where(r => r.Vehicle.OwnerId == userId)
                .Select(r => new OwnerNotificationDto
                {
                    Type = "Review",
                    VehicleId = r.VehicleId,
                    VehiclePlate = r.Vehicle.Plate,
                    BrandName = r.Vehicle.Brand != null ? r.Vehicle.Brand.Name : string.Empty,
                    UserFullName = r.AppUser != null ? r.AppUser.FullName : "Anonim",
                    Message = r.Comment,
                    Rating = r.Rating,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            var notifications = questionNotifications
                .Concat(reviewNotifications)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            return Ok(notifications);
        }
    }
}

