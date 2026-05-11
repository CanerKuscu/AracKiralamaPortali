namespace AracKiralamaPortali.API.DTOs
{
    public class OwnerNotificationDto
    {
        public string Type { get; set; } = string.Empty;
        public int VehicleId { get; set; }
        public string VehiclePlate { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public string? Message { get; set; }
        public int? Rating { get; set; }
        public bool IsAnswered { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
