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
    public class VehiclesController : ControllerBase
    {
        private readonly IRepository<Vehicle> _vehicleRepository;

        public VehiclesController(IRepository<Vehicle> vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }

        private VehicleDto MapToDto(Vehicle v) => new VehicleDto
        {
            Id = v.Id, Model = v.Model, Plate = v.Plate, Year = v.Year, Color = v.Color,
            DailyPrice = v.DailyPrice, WeeklyPrice = v.WeeklyPrice, MonthlyPrice = v.MonthlyPrice,
            Description = v.Description, ImageUrl = v.ImageUrl, VehicleStatus = v.VehicleStatus,
            IsActive = v.IsActive, CreatedAt = v.CreatedAt, FuelType = v.FuelType,
            TransmissionType = v.TransmissionType, Mileage = v.Mileage,
            PassengerCapacity = v.PassengerCapacity, LuggageCapacity = v.LuggageCapacity,
            BrandId = v.BrandId, BrandName = v.Brand.Name
        };

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var vehicles = await _vehicleRepository.GetQueryable().Include(v => v.Brand).ToListAsync();
            return Ok(vehicles.Select(MapToDto));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var vehicle = await _vehicleRepository.GetQueryable().Include(v => v.Brand).FirstOrDefaultAsync(v => v.Id == id);
            if (vehicle == null) return NotFound();
            return Ok(MapToDto(vehicle));
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable()
        {
            var vehicles = await _vehicleRepository.GetQueryable().Include(v => v.Brand)
                .Where(v => v.VehicleStatus == "Available" && v.IsActive).ToListAsync();
            return Ok(vehicles.Select(MapToDto));
        }

        [HttpGet("brand/{brandId}")]
        public async Task<IActionResult> GetByBrand(int brandId)
        {
            var vehicles = await _vehicleRepository.GetQueryable().Include(v => v.Brand)
                .Where(v => v.BrandId == brandId).ToListAsync();
            return Ok(vehicles.Select(MapToDto));
        }

        [HttpGet("filter")]
        public async Task<IActionResult> Filter([FromQuery] string? fuelType, [FromQuery] string? transmissionType,
            [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice, [FromQuery] int? brandId,
            [FromQuery] int? minPassenger)
        {
            var query = _vehicleRepository.GetQueryable().Include(v => v.Brand)
                .Where(v => v.VehicleStatus == "Available" && v.IsActive);

            if (!string.IsNullOrEmpty(fuelType)) query = query.Where(v => v.FuelType == fuelType);
            if (!string.IsNullOrEmpty(transmissionType)) query = query.Where(v => v.TransmissionType == transmissionType);
            if (minPrice.HasValue) query = query.Where(v => v.DailyPrice >= minPrice);
            if (maxPrice.HasValue) query = query.Where(v => v.DailyPrice <= maxPrice);
            if (brandId.HasValue) query = query.Where(v => v.BrandId == brandId);
            if (minPassenger.HasValue) query = query.Where(v => v.PassengerCapacity >= minPassenger);

            return Ok((await query.ToListAsync()).Select(MapToDto));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VehicleCreateDto dto)
        {
            var vehicle = new Vehicle
            {
                Model = dto.Model, Plate = dto.Plate, Year = dto.Year, Color = dto.Color,
                DailyPrice = dto.DailyPrice, WeeklyPrice = dto.WeeklyPrice, MonthlyPrice = dto.MonthlyPrice,
                Description = dto.Description, ImageUrl = dto.ImageUrl, FuelType = dto.FuelType,
                TransmissionType = dto.TransmissionType, Mileage = dto.Mileage,
                PassengerCapacity = dto.PassengerCapacity, LuggageCapacity = dto.LuggageCapacity,
                BrandId = dto.BrandId
            };
            await _vehicleRepository.AddAsync(vehicle);
            await _vehicleRepository.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = vehicle.Id }, dto);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] VehicleUpdateDto dto)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id);
            if (vehicle == null) return NotFound();

            vehicle.Model = dto.Model; vehicle.Plate = dto.Plate; vehicle.Year = dto.Year; vehicle.Color = dto.Color;
            vehicle.DailyPrice = dto.DailyPrice; vehicle.WeeklyPrice = dto.WeeklyPrice; vehicle.MonthlyPrice = dto.MonthlyPrice;
            vehicle.Description = dto.Description; vehicle.ImageUrl = dto.ImageUrl;
            vehicle.VehicleStatus = dto.VehicleStatus; vehicle.IsActive = dto.IsActive;
            vehicle.FuelType = dto.FuelType; vehicle.TransmissionType = dto.TransmissionType;
            vehicle.Mileage = dto.Mileage; vehicle.PassengerCapacity = dto.PassengerCapacity;
            vehicle.LuggageCapacity = dto.LuggageCapacity; vehicle.BrandId = dto.BrandId;

            _vehicleRepository.Update(vehicle);
            await _vehicleRepository.SaveChangesAsync();
            return Ok(new { message = "Vehicle updated successfully." });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id);
            if (vehicle == null) return NotFound();
            _vehicleRepository.Delete(vehicle);
            await _vehicleRepository.SaveChangesAsync();
            return Ok(new { message = "Vehicle deleted successfully." });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("fleet")]
        public async Task<IActionResult> GetFleet()
        {
            var vehicles = await _vehicleRepository.GetQueryable()
                .Include(v => v.Brand)
                .Include(v => v.Reservations.Where(r => r.Status != "Cancelled"))
                    .ThenInclude(r => r.AppUser)
                .ToListAsync();

            var result = vehicles.Select(v => {
                var activeRes = v.Reservations.FirstOrDefault(r => r.Status == "Confirmed" && r.StartDate <= DateTime.Now && r.EndDate >= DateTime.Now);
                return new {
                    v.Id, v.Plate, v.Model, BrandName = v.Brand.Name, v.VehicleStatus,
                    CurrentUser = activeRes != null ? activeRes.AppUser.FullName : null,
                    ReturnDate = activeRes != null ? (DateTime?)activeRes.EndDate : null
                };
            });
            return Ok(result);
        }
    }
}
