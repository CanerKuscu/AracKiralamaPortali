using System.ComponentModel.DataAnnotations;

namespace AracKiralamaPortali.API.DTOs
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public string Status { get; set; } = null!;
        public int ReservationId { get; set; }
    }

    public class PaymentCreateDto
    {
        public decimal Amount { get; set; }

        [Required, MaxLength(50)]
        public string PaymentMethod { get; set; } = null!;

        public int ReservationId { get; set; }
    }

    public class PaymentUpdateDto
    {
        [Required, MaxLength(50)]
        public string Status { get; set; } = null!;
    }
}
