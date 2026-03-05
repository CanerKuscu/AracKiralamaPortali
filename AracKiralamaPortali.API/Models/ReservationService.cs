namespace AracKiralamaPortali.API.Models
{
    public class ReservationService
    {
        public int Id { get; set; }

        public int ReservationId { get; set; }
        public Reservation Reservation { get; set; } = null!;

        public int AdditionalServiceId { get; set; }
        public AdditionalService AdditionalService { get; set; } = null!;
    }
}
