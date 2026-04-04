using System.ComponentModel.DataAnnotations;

namespace AracKiralamaPortali.API.DTOs
{
    public class VehicleImageDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = null!;
        public int DisplayOrder { get; set; }
        public int VehicleId { get; set; }
    }

    public class VehicleImageCreateDto
    {
        [Required]
        public string ImageUrl { get; set; } = null!;

        public int DisplayOrder { get; set; }
        public int VehicleId { get; set; }
    }
}
