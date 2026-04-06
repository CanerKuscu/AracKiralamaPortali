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
    public class UploadController(
        IVehicleImageRepository vehicleImageRepository,
        IVehicleRepository vehicleRepository,
        IWebHostEnvironment hostEnvironment) : ControllerBase
    {

        [HttpPost("vehicle-image")]
        public async Task<IActionResult> UploadVehicleImage(IFormFile file, [FromForm] int vehicleId)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Dosya gereklidir." });

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest(new { message = "Geçersiz dosya türü. Sadece resim dosyalarý kabul edilir." });

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { message = "Dosya boyutu çok büyük. Maksimum 5MB." });

            try
            {
                // Verify vehicle exists
                var vehicle = await vehicleRepository.GetByIdAsync(vehicleId);
                if (vehicle == null)
                    return NotFound(new { message = "Araç bulunamadý." });

                // Create upload directory if it doesn't exist
                var webRootPath = hostEnvironment.WebRootPath ?? Path.Combine(hostEnvironment.ContentRootPath, "wwwroot");
                var uploadsFolder = Path.Combine(webRootPath, "uploads", "vehicles");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Generate unique filename
                var fileName = $"{vehicleId}_{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Get the maximum display order for this vehicle
                // Do not rely on vehicle.Images navigation, because GetByIdAsync does not include related entities.
                var maxDisplayOrder = await vehicleImageRepository.GetQueryable()
                    .Where(i => i.VehicleId == vehicleId)
                    .Select(i => (int?)i.DisplayOrder)
                    .MaxAsync();
                var displayOrder = (maxDisplayOrder ?? 0) + 1;

                // Create database record
                var imageUrl = $"/uploads/vehicles/{fileName}";
                var vehicleImage = new VehicleImage
                {
                    VehicleId = vehicleId,
                    ImageUrl = imageUrl,
                    DisplayOrder = displayOrder
                };

                await vehicleImageRepository.AddAsync(vehicleImage);
                await vehicleImageRepository.SaveChangesAsync();

                return Ok(new VehicleImageDto
                {
                    Id = vehicleImage.Id,
                    ImageUrl = vehicleImage.ImageUrl,
                    DisplayOrder = vehicleImage.DisplayOrder,
                    VehicleId = vehicleImage.VehicleId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Dosya yüklenirken hata oluþtu.", error = ex.Message });
            }
        }
    }
}
