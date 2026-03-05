using AracKiralamaPortali.API.DTOs;
using AracKiralamaPortali.API.Models;
using AracKiralamaPortali.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AracKiralamaPortali.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdditionalServicesController : ControllerBase
    {
        private readonly IRepository<AdditionalService> _repository;

        public AdditionalServicesController(IRepository<AdditionalService> repository)
        {
            _repository = repository;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var services = await _repository.GetAllAsync();
            return Ok(services.Select(s => new AdditionalServiceDto
            {
                Id = s.Id, Name = s.Name, Description = s.Description, Price = s.Price, IsActive = s.IsActive
            }));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var s = await _repository.GetByIdAsync(id);
            if (s == null) return NotFound();
            return Ok(new AdditionalServiceDto { Id = s.Id, Name = s.Name, Description = s.Description, Price = s.Price, IsActive = s.IsActive });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AdditionalServiceCreateDto dto)
        {
            var entity = new AdditionalService { Name = dto.Name, Description = dto.Description, Price = dto.Price };
            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AdditionalServiceUpdateDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return NotFound();
            entity.Name = dto.Name; entity.Description = dto.Description; entity.Price = dto.Price; entity.IsActive = dto.IsActive;
            _repository.Update(entity);
            await _repository.SaveChangesAsync();
            return Ok(new { message = "Service updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return NotFound();
            _repository.Delete(entity);
            await _repository.SaveChangesAsync();
            return Ok(new { message = "Service deleted successfully." });
        }
    }
}
