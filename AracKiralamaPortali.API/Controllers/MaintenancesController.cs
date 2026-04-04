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
    [Authorize(Roles = "Admin,Employee")]
    public class MaintenancesController : ControllerBase
    {
        private readonly IRepository<Maintenance> _repository;

        public MaintenancesController(IRepository<Maintenance> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _repository.GetQueryable().Include(m => m.Vehicle).ThenInclude(v => v.Brand).ToListAsync();
            return Ok(list.Select(m => new MaintenanceDto
            {
                Id = m.Id, Type = m.Type, Description = m.Description, Date = m.Date,
                NextDate = m.NextDate, Cost = m.Cost, MileageAtService = m.MileageAtService,
                Status = m.Status, VehicleId = m.VehicleId, VehiclePlate = m.Vehicle.Plate,
                BrandName = m.Vehicle.Brand.Name
            }));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var m = await _repository.GetQueryable().Include(x => x.Vehicle).ThenInclude(v => v.Brand).FirstOrDefaultAsync(x => x.Id == id);
            if (m == null) return NotFound();
            return Ok(new MaintenanceDto
            {
                Id = m.Id, Type = m.Type, Description = m.Description, Date = m.Date,
                NextDate = m.NextDate, Cost = m.Cost, MileageAtService = m.MileageAtService,
                Status = m.Status, VehicleId = m.VehicleId, VehiclePlate = m.Vehicle.Plate,
                BrandName = m.Vehicle.Brand.Name
            });
        }

        [HttpGet("vehicle/{vehicleId}")]
        public async Task<IActionResult> GetByVehicle(int vehicleId)
        {
            var list = await _repository.GetQueryable().Include(m => m.Vehicle).ThenInclude(v => v.Brand)
                .Where(m => m.VehicleId == vehicleId).ToListAsync();
            return Ok(list.Select(m => new MaintenanceDto
            {
                Id = m.Id, Type = m.Type, Description = m.Description, Date = m.Date,
                NextDate = m.NextDate, Cost = m.Cost, MileageAtService = m.MileageAtService,
                Status = m.Status, VehicleId = m.VehicleId, VehiclePlate = m.Vehicle.Plate,
                BrandName = m.Vehicle.Brand.Name
            }));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MaintenanceCreateDto dto)
        {
            var entity = new Maintenance
            {
                Type = dto.Type, Description = dto.Description, Date = dto.Date,
                NextDate = dto.NextDate, Cost = dto.Cost, MileageAtService = dto.MileageAtService,
                VehicleId = dto.VehicleId
            };
            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MaintenanceUpdateDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return NotFound();
            entity.Type = dto.Type; entity.Description = dto.Description; entity.Date = dto.Date;
            entity.NextDate = dto.NextDate; entity.Cost = dto.Cost;
            entity.MileageAtService = dto.MileageAtService; entity.Status = dto.Status;
            _repository.Update(entity);
            await _repository.SaveChangesAsync();
            return Ok(new { message = "Maintenance updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return NotFound();
            _repository.Delete(entity);
            await _repository.SaveChangesAsync();
            return Ok(new { message = "Maintenance deleted successfully." });
        }

        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcoming()
        {
            var list = await _repository.GetQueryable().Include(m => m.Vehicle).ThenInclude(v => v.Brand)
                .Where(m => m.NextDate != null && m.NextDate <= DateTime.Now.AddDays(30))
                .ToListAsync();
            return Ok(list.Select(m => new MaintenanceDto
            {
                Id = m.Id, Type = m.Type, Description = m.Description, Date = m.Date,
                NextDate = m.NextDate, Cost = m.Cost, MileageAtService = m.MileageAtService,
                Status = m.Status, VehicleId = m.VehicleId, VehiclePlate = m.Vehicle.Plate,
                BrandName = m.Vehicle.Brand.Name
            }));
        }
    }
}
