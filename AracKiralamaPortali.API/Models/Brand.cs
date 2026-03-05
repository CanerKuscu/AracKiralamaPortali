using System.ComponentModel.DataAnnotations;

namespace AracKiralamaPortali.API.Models
{
    public class Brand
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}
