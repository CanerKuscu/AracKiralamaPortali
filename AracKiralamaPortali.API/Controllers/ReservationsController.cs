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
    [Authorize]
    public class ReservationsController : ControllerBase
    {
        private readonly IRepository<Reservation> _reservationRepository;
        private readonly IRepository<Vehicle> _vehicleRepository;
        private readonly IRepository<AdditionalService> _serviceRepository;

        public ReservationsController(
            IRepository<Reservation> reservationRepository,
            IRepository<Vehicle> vehicleRepository,
            IRepository<AdditionalService> serviceRepository)
        {
            _reservationRepository = reservationRepository;
            _vehicleRepository = vehicleRepository;
            _serviceRepository = serviceRepository;
        }

        private ReservationDto MapToDto(Reservation r) => new ReservationDto
        {
            Id = r.Id, StartDate = r.StartDate, EndDate = r.EndDate, TotalPrice = r.TotalPrice,
            DepositAmount = r.DepositAmount, Status = r.Status, PickupLocation = r.PickupLocation,
            DropoffLocation = r.DropoffLocation, CreatedAt = r.CreatedAt, Notes = r.Notes,
            AppUserId = r.AppUserId, UserFullName = r.AppUser.FullName, VehicleId = r.VehicleId,
            VehiclePlate = r.Vehicle.Plate, BrandName = r.Vehicle.Brand.Name,
            AdditionalServices = r.ReservationServices.Select(rs => rs.AdditionalService.Name).ToList()
        };

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var reservations = await _reservationRepository.GetQueryable()
                .Include(r => r.AppUser).Include(r => r.Vehicle).ThenInclude(v => v.Brand)
                .Include(r => r.ReservationServices).ThenInclude(rs => rs.AdditionalService)
                .ToListAsync();
            return Ok(reservations.Select(MapToDto));
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyReservations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reservations = await _reservationRepository.GetQueryable()
                .Include(r => r.Vehicle).ThenInclude(v => v.Brand).Include(r => r.AppUser)
                .Include(r => r.ReservationServices).ThenInclude(rs => rs.AdditionalService)
                .Where(r => r.AppUserId == userId).ToListAsync();
            return Ok(reservations.Select(MapToDto));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var reservation = await _reservationRepository.GetQueryable()
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
            var vehicle = await _vehicleRepository.GetByIdAsync(dto.VehicleId);
            if (vehicle == null) return NotFound(new { message = "Vehicle not found." });
            if (vehicle.VehicleStatus != "Available" || !vehicle.IsActive)
                return BadRequest(new { message = "Vehicle is not available." });

            var hasConflict = await _reservationRepository.AnyAsync(r =>
                r.VehicleId == dto.VehicleId && r.Status != "Cancelled" &&
                r.StartDate < dto.EndDate && r.EndDate > dto.StartDate);
            if (hasConflict) return BadRequest(new { message = "Vehicle is already reserved for the selected dates." });

            var days = (dto.EndDate - dto.StartDate).Days;
            if (days <= 0) return BadRequest(new { message = "End date must be after start date." });

            decimal totalPrice = vehicle.DailyPrice * days;
            decimal serviceTotal = 0;

            var reservation = new Reservation
            {
                StartDate = dto.StartDate, EndDate = dto.EndDate,
                Notes = dto.Notes, PickupLocation = dto.PickupLocation,
                DropoffLocation = dto.DropoffLocation,
                AppUserId = userId!, VehicleId = dto.VehicleId,
                DepositAmount = vehicle.DailyPrice * 3
            };

            if (dto.AdditionalServiceIds != null && dto.AdditionalServiceIds.Any())
            {
                foreach (var svcId in dto.AdditionalServiceIds)
                {
                    var svc = await _serviceRepository.GetByIdAsync(svcId);
                    if (svc != null && svc.IsActive)
                    {
                        reservation.ReservationServices.Add(new ReservationService { AdditionalServiceId = svcId });
                        serviceTotal += svc.Price * days;
                    }
                }
            }

            reservation.TotalPrice = totalPrice + serviceTotal;

            await _reservationRepository.AddAsync(reservation);
            await _reservationRepository.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = reservation.Id },
                new { reservation.Id, reservation.TotalPrice, reservation.DepositAmount });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ReservationUpdateDto dto)
        {
            var reservation = await _reservationRepository.GetByIdAsync(id);
            if (reservation == null) return NotFound();
            reservation.StartDate = dto.StartDate; reservation.EndDate = dto.EndDate;
            reservation.Status = dto.Status; reservation.Notes = dto.Notes;
            _reservationRepository.Update(reservation);
            await _reservationRepository.SaveChangesAsync();
            return Ok(new { message = "Reservation updated successfully." });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var reservation = await _reservationRepository.GetByIdAsync(id);
            if (reservation == null) return NotFound();
            _reservationRepository.Delete(reservation);
            await _reservationRepository.SaveChangesAsync();
            return Ok(new { message = "Reservation deleted successfully." });
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var reservation = await _reservationRepository.GetByIdAsync(id);
            if (reservation == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (reservation.AppUserId != userId && !User.IsInRole("Admin")) return Forbid();
            if (reservation.Status == "Cancelled") return BadRequest(new { message = "Reservation is already cancelled." });
            reservation.Status = "Cancelled";
            _reservationRepository.Update(reservation);
            await _reservationRepository.SaveChangesAsync();
            return Ok(new { message = "Reservation cancelled successfully." });
        }
    }
}
