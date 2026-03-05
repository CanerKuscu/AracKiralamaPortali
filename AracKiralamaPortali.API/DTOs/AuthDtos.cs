using System.ComponentModel.DataAnnotations;

namespace AracKiralamaPortali.API.DTOs
{
    public class RegisterDto
    {
        [Required]
        public string FullName { get; set; } = null!;

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string UserName { get; set; } = null!;

        [Required, MinLength(6)]
        public string Password { get; set; } = null!;

        public string? PhoneNumber { get; set; }
        public string? TCKimlik { get; set; }
        public string? Address { get; set; }
        public string? LicenseClass { get; set; }
        public DateTime? LicenseIssueDate { get; set; }
    }

    public class LoginDto
    {
        [Required]
        public string UserName { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }

    public class UserDto
    {
        public string Id { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? TCKimlik { get; set; }
        public string? Address { get; set; }
        public string? LicenseClass { get; set; }
        public DateTime? LicenseIssueDate { get; set; }
        public bool IsBlackListed { get; set; }
        public string? BlackListReason { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class UserUpdateDto
    {
        [Required]
        public string FullName { get; set; } = null!;

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        public string? PhoneNumber { get; set; }
        public string? TCKimlik { get; set; }
        public string? Address { get; set; }
        public string? LicenseClass { get; set; }
        public DateTime? LicenseIssueDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsBlackListed { get; set; }
        public string? BlackListReason { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; } = null!;

        [Required, MinLength(6)]
        public string NewPassword { get; set; } = null!;
    }

    public class RoleAssignDto
    {
        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        public string RoleName { get; set; } = null!;
    }
}
