using System.Security.Claims;
using AracKiralamaPortali.API.DTOs;
using AracKiralamaPortali.API.Models;
using AracKiralamaPortali.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace AracKiralamaPortali.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReservationsController(
        IReservationRepository reservationRepository,
        IVehicleRepository vehicleRepository,
        IAdditionalServiceRepository serviceRepository,
        IPaymentRepository paymentRepository,
        IReviewRepository reviewRepository) : ControllerBase
    {
        private ReservationDto MapToDto(Reservation r) => new()
        {
            Id = r.Id, StartDate = r.StartDate, EndDate = r.EndDate, TotalPrice = r.TotalPrice,
            DepositAmount = r.DepositAmount, Status = r.Status, PickupLocation = r.PickupLocation,
            DropoffLocation = r.DropoffLocation, CurrentLocationText = r.CurrentLocationText,
            CurrentLatitude = r.CurrentLatitude, CurrentLongitude = r.CurrentLongitude,
            LocationUpdatedAt = r.LocationUpdatedAt, CreatedAt = r.CreatedAt, Notes = r.Notes,
            AppUserId = r.AppUserId, UserFullName = r.AppUser.FullName, VehicleId = r.VehicleId,
            VehiclePlate = r.Vehicle.Plate, BrandName = r.Vehicle.Brand.Name,
            AdditionalServices = [.. r.ReservationServices.Select(rs => rs.AdditionalService.Name)],
            IsReturnConfirmed = r.IsReturnConfirmed
        };

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var reservations = await reservationRepository.GetQueryable()
                .Include(r => r.AppUser).Include(r => r.Vehicle).ThenInclude(v => v.Brand)
                .Include(r => r.ReservationServices).ThenInclude(rs => rs.AdditionalService)
                .ToListAsync();
            return Ok(reservations.Select(MapToDto));
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyReservations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reservations = await reservationRepository.GetQueryable()
                .Include(r => r.Vehicle).ThenInclude(v => v.Brand).Include(r => r.AppUser)
                .Include(r => r.ReservationServices).ThenInclude(rs => rs.AdditionalService)
                .Where(r => r.AppUserId == userId).ToListAsync();
            return Ok(reservations.Select(MapToDto));
        }

        [HttpGet("owner-reservations")]
        public async Task<IActionResult> GetOwnerReservations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reservations = await reservationRepository.GetQueryable()
                .Include(r => r.Vehicle).ThenInclude(v => v.Brand).Include(r => r.AppUser)
                .Include(r => r.ReservationServices).ThenInclude(rs => rs.AdditionalService)
                .Where(r => r.Vehicle.OwnerId == userId).ToListAsync();
            return Ok(reservations.Select(MapToDto));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var reservation = await reservationRepository.GetQueryable()
                .Include(r => r.AppUser).Include(r => r.Vehicle).ThenInclude(v => v.Brand)
                .Include(r => r.ReservationServices).ThenInclude(rs => rs.AdditionalService)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (reservation == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (reservation.AppUserId != userId && !User.IsInRole("Admin")) return Forbid();

            return Ok(MapToDto(reservation));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReservationCreateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var startDate = dto.StartDate.Date;
            var endDate = dto.EndDate.Date;

            if (startDate < DateTime.Today)
                return BadRequest(new { message = "Baslangic tarihi bugunden önce olamaz." });

            if (endDate <= startDate)
                return BadRequest(new { message = "Bitis tarihi baslangic tarihinden sonra olmalidir." });

            var vehicle = await vehicleRepository.GetQueryable()
                .Include(v => v.Reservations)
                .FirstOrDefaultAsync(v => v.Id == dto.VehicleId);
            
            if (vehicle == null) return NotFound(new { message = "Araç bulunamadý." });
            if (!vehicle.IsActive)
                return BadRequest(new { message = "Araç þu anda kiralamaya uygun deðil." });
            if (!string.IsNullOrEmpty(vehicle.OwnerId) && vehicle.OwnerId == userId)
            if (!string.IsNullOrEmpty(vehicle.OwnerId) &&
                string.Equals(vehicle.OwnerId, userId, StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "Kendi aracýnýz için rezervasyon yapamazsýnýz." });

            // Þu an kirada olan rezervasyonu kontrol et
            var currentRental = vehicle.Reservations.FirstOrDefault(r =>
                r.Status == "Confirmed" && r.StartDate <= DateTime.Now && r.EndDate >= DateTime.Now);
            
            if (currentRental != null)
                return BadRequest(new { message = $"Araç {currentRental.EndDate:yyyy-MM-dd HH:mm} tarihine kadar kirada. Lütfen bu tarihten sonrasýný seçin." });

            // Ýstenen tarihler için çakýþma kontrol et
            var hasConflict = await reservationRepository.AnyAsync(r =>
                r.VehicleId == dto.VehicleId && r.Status == "Confirmed" &&
                r.StartDate < dto.EndDate && r.EndDate > dto.StartDate);
            if (hasConflict) return BadRequest(new { message = "Araç seçilen tarihlerde zaten rezerve edilmiþ." });

            var days = (dto.EndDate - dto.StartDate).Days;
            if (days <= 0) return BadRequest(new { message = "Bitiþ tarihi baþlangýç tarihinden sonra olmalýdýr." });

            decimal totalPrice;
            if (days >= 30 && vehicle.MonthlyPrice.HasValue)
            {
                int months = days / 30;
                int remainingDays = days % 30;
                totalPrice = (vehicle.MonthlyPrice.Value * months) + (vehicle.DailyPrice * remainingDays);
            }
            else if (days >= 7 && vehicle.WeeklyPrice.HasValue)
            {
                int weeks = days / 7;
                int remainingDays = days % 7;
                totalPrice = (vehicle.WeeklyPrice.Value * weeks) + (vehicle.DailyPrice * remainingDays);
            }
            else
            {
                totalPrice = vehicle.DailyPrice * days;
            }
            decimal serviceTotal = 0;

            var reservation = new Reservation
            {
                StartDate = startDate, EndDate = endDate,
                Notes = dto.Notes, PickupLocation = dto.PickupLocation,
                DropoffLocation = dto.DropoffLocation,
                AppUserId = userId!, VehicleId = dto.VehicleId,
                DepositAmount = vehicle.DailyPrice * 3
            };

            if (dto.AdditionalServiceIds is { Count: > 0 })
            {
                foreach (var svcId in dto.AdditionalServiceIds)
                {
                    var svc = await serviceRepository.GetByIdAsync(svcId);
                    if (svc != null && svc.IsActive)
                    {
                        reservation.ReservationServices.Add(new ReservationService { AdditionalServiceId = svcId });
                        serviceTotal += svc.Price * days;
                    }
                }
            }

            reservation.TotalPrice = totalPrice + serviceTotal;

            await reservationRepository.AddAsync(reservation);
            await reservationRepository.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = reservation.Id },
                new { reservation.Id, reservation.TotalPrice, reservation.DepositAmount });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ReservationUpdateDto dto)
        {
            var reservation = await reservationRepository.GetByIdAsync(id);
            if (reservation == null) return NotFound();
            reservation.StartDate = dto.StartDate; reservation.EndDate = dto.EndDate;
            reservation.Status = dto.Status; reservation.Notes = dto.Notes;
            reservationRepository.Update(reservation);
            await reservationRepository.SaveChangesAsync();
            return Ok(new { message = "Rezervasyon baþarýyla güncellendi." });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var reservation = await reservationRepository.GetQueryable()
                .Include(r => r.ReservationServices)
                .Include(r => r.Payment)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null) return NotFound();

            var reviews = await reviewRepository.GetQueryable()
                .Where(r => r.ReservationId == id)
                .ToListAsync();

            foreach (var review in reviews)
                reviewRepository.Delete(review);

            if (reservation.Payment != null)
                paymentRepository.Delete(reservation.Payment);

            foreach (var reservationService in reservation.ReservationServices.ToList())
                reservation.ReservationServices.Remove(reservationService);

            reservationRepository.Delete(reservation);
            await reservationRepository.SaveChangesAsync();
            return Ok(new { message = "Rezervasyon baþarýyla silindi." });
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var reservation = await reservationRepository.GetByIdAsync(id);
            if (reservation == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (reservation.AppUserId != userId && !User.IsInRole("Admin")) return Forbid();
            if (reservation.Status == "Cancelled") return BadRequest(new { message = "Rezervasyon zaten iptal edilmiþ." });
            reservation.Status = "Cancelled";
            reservationRepository.Update(reservation);
            await reservationRepository.SaveChangesAsync();
            return Ok(new { message = "Rezervasyon baaryla iptal edildi." });
        }

        [HttpPut("{id}/request-return")]
        public async Task<IActionResult> RequestReturn(int id)
        {
            var reservation = await reservationRepository.GetByIdAsync(id);
            if (reservation == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (reservation.AppUserId != userId) return Forbid();

            if (reservation.Status != "Confirmed")
            {
                return BadRequest(new { message = "Sadece onaylý ve devam eden rezervasyonlar için teslim talebinde bulunabilirsiniz." });
            }

            reservation.Status = "ReturnPending";
            reservationRepository.Update(reservation);
            await reservationRepository.SaveChangesAsync();

            return Ok(new { message = "Araç teslim etme talebiniz alýndý. Araç sahibinin onayý bekleniyor." });
        }

        [HttpPut("{id}/confirm-return")]
        public async Task<IActionResult> ConfirmReturn(int id)
        {
            var reservation = await reservationRepository.GetQueryable()
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Sadece araba sahibi veya Admin iade onaylayabilir
            if (reservation.Vehicle.OwnerId != userId && !User.IsInRole("Admin"))
                return Forbid();

            if (reservation.Status != "Confirmed" && reservation.Status != "ReturnPending" && reservation.Status != "Completed")
            {
                return BadRequest(new { message = "Sadece bitmiþ veya onaylý rezervasyonlar için iade onaylanabilir." });
            }

            if (reservation.IsReturnConfirmed)
            {
                return BadRequest(new { message = "Araç iadesi zaten onaylanmýþ." });
            }

            // Ýade onaylandý olarak iþaretle ve durumu Completed yap
            reservation.IsReturnConfirmed = true;
            reservation.Status = "Completed";
            reservationRepository.Update(reservation);

            // Aracý tekrar müsait duruma getir
            reservation.Vehicle.IsActive = true;
            reservation.Vehicle.VehicleStatus = "Available"; // Opsiyonel, eðer durum alanýnýz varsa
            vehicleRepository.Update(reservation.Vehicle);

            await reservationRepository.SaveChangesAsync();
            await vehicleRepository.SaveChangesAsync();

            return Ok(new { message = "Ara iadesi baaryla onayland ve ara tekrar aktif hale getirildi." });
        }
    }
}

