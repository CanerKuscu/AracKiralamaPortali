using System.ComponentModel.DataAnnotations;

namespace AracKiralamaPortali.API.DTOs
{
    public class AdditionalServiceDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
    }

    public class AdditionalServiceCreateDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(300)]
        public string? Description { get; set; }

        public decimal Price { get; set; }
    }

    public class AdditionalServiceUpdateDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(300)]
        public string? Description { get; set; }

        public decimal Price { get; set; }
        public bool IsActive { get; set; }
    }
}
