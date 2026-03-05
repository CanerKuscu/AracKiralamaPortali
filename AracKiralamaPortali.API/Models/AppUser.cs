using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AracKiralamaPortali.API.Models
{
    public class AppUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; } = null!;

        [MaxLength(11)]
        public string? TCKimlik { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(10)]
        public string? LicenseClass { get; set; }

        public DateTime? LicenseIssueDate { get; set; }

        public bool IsBlackListed { get; set; } = false;

        [MaxLength(500)]
        public string? BlackListReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
