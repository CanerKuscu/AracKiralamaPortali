using System.ComponentModel.DataAnnotations;

namespace AracKiralamaPortali.API.DTOs
{
    public class BrandDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class BrandCreateDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;
    }

    public class BrandUpdateDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        public bool IsActive { get; set; }
    }
}
