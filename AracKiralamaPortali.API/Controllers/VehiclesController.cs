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
    public class VehiclesController : ControllerBase
    {
        private readonly IRepository<Vehicle> _vehicleRepository;
        private readonly IRepository<Brand> _brandRepository;

        public VehiclesController(IRepository<Vehicle> vehicleRepository, IRepository<Brand> brandRepository)
        {
            _vehicleRepository = vehicleRepository;
            _brandRepository = brandRepository;
        }

        private VehicleDto MapToDto(Vehicle v) => new VehicleDto
        {
            Id = v.Id, Model = v.Model, Plate = v.Plate, Year = v.Year, Color = v.Color,
            DailyPrice = v.DailyPrice, WeeklyPrice = v.WeeklyPrice, MonthlyPrice = v.MonthlyPrice,
            Description = v.Description, ImageUrl = v.ImageUrl, VehicleStatus = v.VehicleStatus,
            IsActive = v.IsActive, CreatedAt = v.CreatedAt, FuelType = v.FuelType,
            TransmissionType = v.TransmissionType, Mileage = v.Mileage,
            PassengerCapacity = v.PassengerCapacity, LuggageCapacity = v.LuggageCapacity,
            BrandId = v.BrandId, BrandName = v.Brand.Name,
            InsuranceExpiryDate = v.InsuranceExpiryDate,
            InspectionExpiryDate = v.InspectionExpiryDate,
            HasAccidentHistory = v.HasAccidentHistory,
            AverageRating = v.Reviews.Count > 0 ? v.Reviews.Average(r => r.Rating) : 0,
            ReviewCount = v.Reviews.Count,
            ImageUrls = v.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToList(),
            OwnerId = v.OwnerId
        };

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var vehicles = await _vehicleRepository.GetQueryable()
                .Include(v => v.Brand).Include(v => v.Reviews).Include(v => v.Images)
                .Include(v => v.Reservations)
                .ToListAsync();
            
            var dtos = vehicles.Select(v => {
                var dto = MapToDto(v);
                
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
            var vehicle = await _vehicleRepository.GetQueryable()
                .Include(v => v.Brand).Include(v => v.Reviews).Include(v => v.Images)
                .Include(v => v.Reservations)
                .FirstOrDefaultAsync(v => v.Id == id);
            if (vehicle == null) return NotFound();
            
            var dto = MapToDto(vehicle);
            
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
            var vehicles = await _vehicleRepository.GetQueryable()
                .Include(v => v.Brand).Include(v => v.Reviews).Include(v => v.Images)
                .Include(v => v.Reservations)
                .ToListAsync();
            
            var availableVehicles = vehicles.Where(v => v.IsActive && 
                (v.VehicleStatus == "Available" || v.VehicleStatus == "Rented")).ToList()
                .Where(v => !v.Reservations.Any(r => r.Status != "Cancelled" && 
                    r.StartDate <= DateTime.Now && r.EndDate >= DateTime.Now))
                .ToList();
            
            return Ok(availableVehicles.Select(MapToDto));
        }

        [HttpGet("brand/{brandId}")]
        public async Task<IActionResult> GetByBrand(int brandId)
        {
            var vehicles = await _vehicleRepository.GetQueryable()
                .Include(v => v.Brand).Include(v => v.Reviews).Include(v => v.Images)
                .Include(v => v.Reservations)
                .Where(v => v.BrandId == brandId).ToListAsync();
            
            var dtos = vehicles.Select(v => {
                var dto = MapToDto(v);
                
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
        public async Task<IActionResult> Filter([FromQuery] string? fuelType, [FromQuery] string? transmissionType,
            [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice, [FromQuery] int? brandId,
            [FromQuery] int? minPassenger)
        {
            var query = _vehicleRepository.GetQueryable()
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
                !v.Reservations.Any(r => r.Status != "Cancelled" && 
                    r.StartDate <= DateTime.Now && r.EndDate >= DateTime.Now))
                .ToList();

            return Ok(availableVehicles.Select(MapToDto));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VehicleCreateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            int resolvedBrandId = dto.BrandId;
            if (!string.IsNullOrWhiteSpace(dto.BrandName))
            {
                var existingBrand = await _brandRepository.GetQueryable()
                    .FirstOrDefaultAsync(b => b.Name.ToLower() == dto.BrandName.Trim().ToLower());
                
                if (existingBrand != null)
                {
                    resolvedBrandId = existingBrand.Id;
                }
                else
                {
                    var newBrand = new Brand { Name = dto.BrandName.Trim(), IsActive = true };
                    await _brandRepository.AddAsync(newBrand);
                    await _brandRepository.SaveChangesAsync();
                    resolvedBrandId = newBrand.Id;
                }
            }

            var vehicle = new Vehicle
            {
                Model = dto.Model, Plate = dto.Plate, Year = dto.Year, Color = dto.Color,
                DailyPrice = dto.DailyPrice, WeeklyPrice = dto.WeeklyPrice, MonthlyPrice = dto.MonthlyPrice,
                Description = dto.Description, ImageUrl = dto.ImageUrl, FuelType = dto.FuelType,
                TransmissionType = dto.TransmissionType, Mileage = dto.Mileage,
                PassengerCapacity = dto.PassengerCapacity, LuggageCapacity = dto.LuggageCapacity,
                BrandId = resolvedBrandId,
                InsuranceExpiryDate = dto.InsuranceExpiryDate,
                InspectionExpiryDate = dto.InspectionExpiryDate,
                HasAccidentHistory = dto.HasAccidentHistory,
                OwnerId = userId
            };
            await _vehicleRepository.AddAsync(vehicle);
            await _vehicleRepository.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = vehicle.Id }, new { id = vehicle.Id, message = "Araç baþarýyla oluþturuldu." });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] VehicleUpdateDto dto)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id);
            if (vehicle == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            
            if (vehicle.OwnerId != userId && userRole != "Admin")
                return Forbid();

            int resolvedBrandId = dto.BrandId;
            if (!string.IsNullOrWhiteSpace(dto.BrandName))
            {
                var existingBrand = await _brandRepository.GetQueryable()
                    .FirstOrDefaultAsync(b => b.Name.ToLower() == dto.BrandName.Trim().ToLower());
                
                if (existingBrand != null)
                {
                    resolvedBrandId = existingBrand.Id;
                }
                else
                {
                    var newBrand = new Brand { Name = dto.BrandName.Trim(), IsActive = true };
                    await _brandRepository.AddAsync(newBrand);
                    await _brandRepository.SaveChangesAsync();
                    resolvedBrandId = newBrand.Id;
                }
            }

            vehicle.Model = dto.Model; vehicle.Plate = dto.Plate; vehicle.Year = dto.Year; vehicle.Color = dto.Color;
            vehicle.DailyPrice = dto.DailyPrice; vehicle.WeeklyPrice = dto.WeeklyPrice; vehicle.MonthlyPrice = dto.MonthlyPrice;
            vehicle.Description = dto.Description; vehicle.ImageUrl = dto.ImageUrl;
            vehicle.VehicleStatus = dto.VehicleStatus; vehicle.IsActive = dto.IsActive;
            vehicle.FuelType = dto.FuelType; vehicle.TransmissionType = dto.TransmissionType;
            vehicle.Mileage = dto.Mileage; vehicle.PassengerCapacity = dto.PassengerCapacity;
            vehicle.LuggageCapacity = dto.LuggageCapacity; vehicle.BrandId = resolvedBrandId;
            vehicle.InsuranceExpiryDate = dto.InsuranceExpiryDate;
            vehicle.InspectionExpiryDate = dto.InspectionExpiryDate;
            vehicle.HasAccidentHistory = dto.HasAccidentHistory;

            _vehicleRepository.Update(vehicle);
            await _vehicleRepository.SaveChangesAsync();
            return Ok(new { message = "Araç baþarýyla güncellendi." });
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id);
            if (vehicle == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            
            if (vehicle.OwnerId != userId && userRole != "Admin")
                return Forbid();

            _vehicleRepository.Delete(vehicle);
            await _vehicleRepository.SaveChangesAsync();
            return Ok(new { message = "Araç baþarýyla silindi." });
        }

        [Authorize(Roles = "Admin,Employee")]
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

