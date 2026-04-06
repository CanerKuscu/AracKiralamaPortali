using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AracKiralamaPortali.API.Data;
using AracKiralamaPortali.API.DTOs;
using AracKiralamaPortali.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AracKiralamaPortali.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration,
        AppDbContext context) : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly SignInManager<AppUser> _signInManager = signInManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly IConfiguration _configuration = configuration;
        private readonly AppDbContext _context = context;

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (dto.LicenseIssueDate.HasValue && dto.LicenseIssueDate.Value.Date > DateTime.Today)
                return BadRequest(new { message = "Ehliyet veriliþ tarihi bugünden ileri bir tarih olamaz." });

            var existingUser = await _userManager.FindByNameAsync(dto.UserName);
            if (existingUser != null)
            {
                if (!existingUser.IsDeleted)
                    return BadRequest(new { message = "This username is already taken." });

                var emailOwner = await _userManager.FindByEmailAsync(dto.Email);
                if (emailOwner != null && emailOwner.Id != existingUser.Id)
                    return BadRequest(new { message = "This email is already taken." });

                existingUser.FullName = dto.FullName;
                existingUser.Email = dto.Email;
                existingUser.UserName = dto.UserName;
                existingUser.PhoneNumber = dto.PhoneNumber;
                existingUser.TCKimlik = dto.TCKimlik;
                existingUser.BirthDate = dto.BirthDate;
                existingUser.Address = dto.Address;
                existingUser.LicenseClass = dto.LicenseClass;
                existingUser.LicenseIssueDate = dto.LicenseIssueDate;
                existingUser.IsDeleted = false;
                existingUser.DeletedAt = null;
                existingUser.IsActive = true;
                existingUser.IsBlackListed = false;
                existingUser.BlackListReason = null;

                var updateResult = await _userManager.UpdateAsync(existingUser);
                if (!updateResult.Succeeded)
                    return BadRequest(updateResult.Errors);

                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(existingUser);
                var resetResult = await _userManager.ResetPasswordAsync(existingUser, resetToken, dto.Password);
                if (!resetResult.Succeeded)
                    return BadRequest(resetResult.Errors);

                var currentRoles = await _userManager.GetRolesAsync(existingUser);
                if (currentRoles.Count > 0)
                {
                    var removeRolesResult = await _userManager.RemoveFromRolesAsync(existingUser, currentRoles);
                    if (!removeRolesResult.Succeeded)
                        return BadRequest(removeRolesResult.Errors);
                }

                var roleToAssignRecovered = "User";
                if (!string.IsNullOrEmpty(dto.Role) && (dto.Role == "User" || dto.Role == "CarOwner"))
                {
                    roleToAssignRecovered = dto.Role;
                }

                var addRoleResult = await _userManager.AddToRoleAsync(existingUser, roleToAssignRecovered);
                if (!addRoleResult.Succeeded)
                    return BadRequest(addRoleResult.Errors);

                return Ok(new { message = "Kayýt iþlemi baþarýlý." });
            }

            var existingEmailUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingEmailUser != null)
                return BadRequest(new { message = "This email is already taken." });

            var user = new AppUser
            {
                FullName = dto.FullName,
                Email = dto.Email,
                UserName = dto.UserName,
                PhoneNumber = dto.PhoneNumber,
                TCKimlik = dto.TCKimlik,
                BirthDate = dto.BirthDate,
                Address = dto.Address,
                LicenseClass = dto.LicenseClass,
                LicenseIssueDate = dto.LicenseIssueDate
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            var roleToAssign = "User";
            if (!string.IsNullOrEmpty(dto.Role) && (dto.Role == "User" || dto.Role == "CarOwner"))
            {
                roleToAssign = dto.Role;
            }

            await _userManager.AddToRoleAsync(user, roleToAssign);
            return Ok(new { message = "Kayýt iþlemi baþarýlý." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || user.IsDeleted)
                return Unauthorized(new { message = "E-posta veya þifre hatalý." });

            if (!user.IsActive)
                return Unauthorized(new { message = "Hesabýnýz pasif durumda." });

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
                return Unauthorized(new { message = "E-posta veya þifre hatalý." });

            var token = await GenerateJwtToken(user);
            return Ok(new { token });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = _userManager.Users.Where(u => !u.IsDeleted).ToList();
            List<UserDto> userDtos = [];

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email!,
                    UserName = user.UserName!,
                    PhoneNumber = user.PhoneNumber,
                    TCKimlik = user.TCKimlik,
                    BirthDate = user.BirthDate,
                    Address = user.Address,
                    LicenseClass = user.LicenseClass,
                    LicenseIssueDate = user.LicenseIssueDate,
                    IsBlackListed = user.IsBlackListed,
                    BlackListReason = user.BlackListReason,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    Roles = [.. roles]
                });
            }

            return Ok(userDtos);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || user.IsDeleted)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var dto = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                UserName = user.UserName!,
                PhoneNumber = user.PhoneNumber,
                TCKimlik = user.TCKimlik,
                BirthDate = user.BirthDate,
                Address = user.Address,
                LicenseClass = user.LicenseClass,
                LicenseIssueDate = user.LicenseIssueDate,
                IsBlackListed = user.IsBlackListed,
                BlackListReason = user.BlackListReason,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                Roles = [.. roles]
            };

            return Ok(dto);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserUpdateDto dto)
        {
            if (dto.LicenseIssueDate.HasValue && dto.LicenseIssueDate.Value.Date > DateTime.Today)
                return BadRequest(new { message = "Ehliyet veriliþ tarihi bugünden ileri bir tarih olamaz." });

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.PhoneNumber = dto.PhoneNumber;
            user.TCKimlik = dto.TCKimlik;
            user.BirthDate = dto.BirthDate;
            user.Address = dto.Address;
            user.LicenseClass = dto.LicenseClass;
            user.LicenseIssueDate = dto.LicenseIssueDate;
            user.IsActive = dto.IsActive;
            user.IsBlackListed = dto.IsBlackListed;
            user.BlackListReason = dto.BlackListReason;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Kullanýcý baþarýyla güncellendi." });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Contains("Admin"))
                return BadRequest(new { message = "Admin hesabý silinemez." });

            // Soft Delete - Veritabanýndan silme deðil, iþaretleme
            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            user.IsActive = false;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Kullanýcý hesabý pasife alýndý." });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null)
                return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Þifre baþarýyla deðiþtirildi." });
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null || user.IsDeleted)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var dto = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                UserName = user.UserName!,
                PhoneNumber = user.PhoneNumber,
                TCKimlik = user.TCKimlik,
                BirthDate = user.BirthDate,
                Address = user.Address,
                LicenseClass = user.LicenseClass,
                LicenseIssueDate = user.LicenseIssueDate,
                IsBlackListed = user.IsBlackListed,
                BlackListReason = user.BlackListReason,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                Roles = [.. roles]
            };

            return Ok(dto);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("roles/assign")]
        public async Task<IActionResult> AssignRole([FromBody] RoleAssignDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
                return NotFound(new { message = "Kullanýcý bulunamadý." });

            if (!await _roleManager.RoleExistsAsync(dto.RoleName))
                return BadRequest(new { message = "Belirtilen rol bulunamadý." });

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            var result = await _userManager.AddToRoleAsync(user, dto.RoleName);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Rol baþarýyla atandý." });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("roles")]
        public IActionResult GetRoles()
        {
            var roles = _roleManager.Roles.Select(r => new { r.Id, r.Name }).ToList();
            return Ok(roles);
        }

        private async Task<string> GenerateJwtToken(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            List<Claim> claims =
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim("FullName", user.FullName)
            ];

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
