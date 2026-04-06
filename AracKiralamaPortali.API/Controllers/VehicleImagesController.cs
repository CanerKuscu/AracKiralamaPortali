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
    public class VehicleImagesController(IVehicleImageRepository repository) : ControllerBase
    {

        [HttpGet("vehicle/{vehicleId}")]
        public async Task<IActionResult> GetByVehicle(int vehicleId)
        {
            var images = await repository.GetQueryable()
                .Where(i => i.VehicleId == vehicleId)
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            return Ok(images.Select(i => new VehicleImageDto
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                DisplayOrder = i.DisplayOrder,
                VehicleId = i.VehicleId
            }));
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VehicleImageCreateDto dto)
        {
            var image = new VehicleImage
            {
                ImageUrl = dto.ImageUrl,
                DisplayOrder = dto.DisplayOrder,
                VehicleId = dto.VehicleId
            };

            await repository.AddAsync(image);
            await repository.SaveChangesAsync();

            return Ok(new VehicleImageDto
            {
                Id = image.Id,
                ImageUrl = image.ImageUrl,
                DisplayOrder = image.DisplayOrder,
                VehicleId = image.VehicleId
            });
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var image = await repository.GetByIdAsync(id);
            if (image == null) return NotFound();

            repository.Delete(image);
            await repository.SaveChangesAsync();
            return Ok(new { message = "Görsel baþarýyla silindi." });
        }
    }
}
