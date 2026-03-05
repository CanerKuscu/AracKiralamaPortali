using AracKiralamaPortali.API.DTOs;
using AracKiralamaPortali.API.Models;
using AracKiralamaPortali.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AracKiralamaPortali.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandsController : ControllerBase
    {
        private readonly IRepository<Brand> _brandRepository;

        public BrandsController(IRepository<Brand> brandRepository)
        {
            _brandRepository = brandRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var brands = await _brandRepository.GetAllAsync();
            var dtos = brands.Select(b => new BrandDto
            {
                Id = b.Id,
                Name = b.Name,
                IsActive = b.IsActive,
                CreatedAt = b.CreatedAt
            });
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null)
                return NotFound();

            var dto = new BrandDto
            {
                Id = brand.Id,
                Name = brand.Name,
                IsActive = brand.IsActive,
                CreatedAt = brand.CreatedAt
            };
            return Ok(dto);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BrandCreateDto dto)
        {
            var brand = new Brand
            {
                Name = dto.Name
            };

            await _brandRepository.AddAsync(brand);
            await _brandRepository.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = brand.Id }, new BrandDto
            {
                Id = brand.Id,
                Name = brand.Name,
                IsActive = brand.IsActive,
                CreatedAt = brand.CreatedAt
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] BrandUpdateDto dto)
        {
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null)
                return NotFound();

            brand.Name = dto.Name;
            brand.IsActive = dto.IsActive;

            _brandRepository.Update(brand);
            await _brandRepository.SaveChangesAsync();

            return Ok(new BrandDto
            {
                Id = brand.Id,
                Name = brand.Name,
                IsActive = brand.IsActive,
                CreatedAt = brand.CreatedAt
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null)
                return NotFound();

            _brandRepository.Delete(brand);
            await _brandRepository.SaveChangesAsync();

            return Ok(new { message = "Brand deleted successfully." });
        }
    }
}
