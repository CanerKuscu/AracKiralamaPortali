using System.Security.Claims;
using AracKiralamaPortali.API.DTOs;
using AracKiralamaPortali.API.Models;
using AracKiralamaPortali.API.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AracKiralamaPortali.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehiclesController(IVehicleRepository vehicleRepository, IBrandRepository brandRepository, IMapper mapper) : ControllerBase
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
                (v.VehicleStatus == "Available" || v.VehicleStatus == "Rented")).ToList()
                .Where(v => !v.Reservations.Any(r => r.Status != "Cancelled" && 
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
        public async Task<IActionResult> Filter([FromQuery] string? fuelType, [FromQuery] string? transmissionType,
            [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice, [FromQuery] int? brandId,
            [FromQuery] int? minPassenger)
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
                !v.Reservations.Any(r => r.Status != "Cancelled" && 
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
    }
}

