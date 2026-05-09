using System.ComponentModel.DataAnnotations;

namespace AracKiralamaPortali.API.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Ad soyad alaný zorunludur.")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "E-posta alaný zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Kullanýcý adý alaný zorunludur.")]
        [RegularExpression(@"^[a-zA-Z0-9_çÇðÐýÝöÖþÞüÜ]{3,20}$",
            ErrorMessage = "Kullanýcý adý 3-20 karakter olmalý ve yalnýzca harf, rakam veya alt çizgi içermelidir.")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Þifre alaný zorunludur.")]
        [MinLength(6, ErrorMessage = "Þifre en az 6 karakter olmalýdýr.")]
        public string Password { get; set; } = null!;

        public string? PhoneNumber { get; set; }
        public string? TCKimlik { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Address { get; set; }
        public string? LicenseClass { get; set; }
        public DateTime? LicenseIssueDate { get; set; }
        public string? Role { get; set; }
    }

    public class LoginDto
    {
        [Required(ErrorMessage = "E-posta alaný zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Þifre alaný zorunludur.")]
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
        public DateTime? BirthDate { get; set; }
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
        [Required(ErrorMessage = "Ad soyad alaný zorunludur.")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "E-posta alaný zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; } = null!;

        public string? PhoneNumber { get; set; }
        public string? TCKimlik { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Address { get; set; }
        public string? LicenseClass { get; set; }
        public DateTime? LicenseIssueDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsBlackListed { get; set; }
        public string? BlackListReason { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Mevcut þifre alaný zorunludur.")]
        public string CurrentPassword { get; set; } = null!;

        [Required(ErrorMessage = "Yeni þifre alaný zorunludur.")]
        [MinLength(6, ErrorMessage = "Yeni þifre en az 6 karakter olmalýdýr.")]
        public string NewPassword { get; set; } = null!;
    }

    public class RoleAssignDto
    {
        [Required(ErrorMessage = "Kullanýcý alaný zorunludur.")]
        public string UserId { get; set; } = null!;

        [Required(ErrorMessage = "Rol alaný zorunludur.")]
        public string RoleName { get; set; } = null!;
    }

    public class DeleteAccountDto
    {
        [Required(ErrorMessage = "Þifre alaný zorunludur.")]
        public string Password { get; set; } = null!;
    }
}
