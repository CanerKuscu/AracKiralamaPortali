using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AracKiralamaPortali.API.Models
{
    public class Payment
    {
        public int Id { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required, MaxLength(50)]
        public string PaymentMethod { get; set; } = null!;

        [Required, MaxLength(50)]
        public string Status { get; set; } = "Completed";

        public int ReservationId { get; set; }
        [JsonIgnore]
        public Reservation Reservation { get; set; } = null!;
    }
}
