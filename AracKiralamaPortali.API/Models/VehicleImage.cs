using System.Text.Json.Serialization;

namespace AracKiralamaPortali.API.Models
{
    public class VehicleImage
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = null!;
        public int DisplayOrder { get; set; }

        public int VehicleId { get; set; }
        [JsonIgnore]
        public Vehicle Vehicle { get; set; } = null!;
    }
}
