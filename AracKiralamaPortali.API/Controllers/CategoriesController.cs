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
    public class CategoriesController(IBrandRepository brandRepository, IVehicleRepository vehicleRepository) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await brandRepository.GetAllAsync();
            var dtos = categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            });

            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await brandRepository.GetByIdAsync(id);
            if (category == null)
                return NotFound();

            var dto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt
            };

            return Ok(dto);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto)
        {
            var category = new Brand
            {
                Name = dto.Name
            };

            await brandRepository.AddAsync(category);
            await brandRepository.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = category.Id }, new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryUpdateDto dto)
        {
            var category = await brandRepository.GetByIdAsync(id);
            if (category == null)
                return NotFound();

            category.Name = dto.Name;
            category.IsActive = dto.IsActive;

            brandRepository.Update(category);
            await brandRepository.SaveChangesAsync();

            return Ok(new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await brandRepository.GetByIdAsync(id);
            if (category == null)
                return NotFound();

            var hasVehicles = await vehicleRepository.GetQueryable().AnyAsync(v => v.BrandId == id);
            if (hasVehicles)
                return BadRequest(new { message = "Bu kategoriye baðlý araçlar olduðu için silinemez." });

            brandRepository.Delete(category);
            await brandRepository.SaveChangesAsync();

            return Ok(new { message = "Category deleted successfully." });
        }
    }
}
