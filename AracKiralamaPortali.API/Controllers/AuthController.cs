using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AracKiralamaPortali.API.DTOs;
using AracKiralamaPortali.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AracKiralamaPortali.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var user = new AppUser
            {
                FullName = dto.FullName,
                Email = dto.Email,
                UserName = dto.UserName,
                PhoneNumber = dto.PhoneNumber,
                TCKimlik = dto.TCKimlik,
                Address = dto.Address,
                LicenseClass = dto.LicenseClass,
                LicenseIssueDate = dto.LicenseIssueDate
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, "User");
            return Ok(new { message = "Registration successful." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);
            if (user == null)
                return Unauthorized(new { message = "Invalid username or password." });

            if (!user.IsActive)
                return Unauthorized(new { message = "Account is deactivated." });

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
                return Unauthorized(new { message = "Invalid username or password." });

            var token = await GenerateJwtToken(user);
            return Ok(new { token });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = _userManager.Users.ToList();
            var userDtos = new List<UserDto>();

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
                    Address = user.Address,
                    LicenseClass = user.LicenseClass,
                    LicenseIssueDate = user.LicenseIssueDate,
                    IsBlackListed = user.IsBlackListed,
                    BlackListReason = user.BlackListReason,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    Roles = roles.ToList()
                });
            }

            return Ok(userDtos);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
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
                Address = user.Address,
                LicenseClass = user.LicenseClass,
                LicenseIssueDate = user.LicenseIssueDate,
                IsBlackListed = user.IsBlackListed,
                BlackListReason = user.BlackListReason,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                Roles = roles.ToList()
            };

            return Ok(dto);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserUpdateDto dto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.PhoneNumber = dto.PhoneNumber;
            user.TCKimlik = dto.TCKimlik;
            user.Address = dto.Address;
            user.LicenseClass = dto.LicenseClass;
            user.LicenseIssueDate = dto.LicenseIssueDate;
            user.IsActive = dto.IsActive;
            user.IsBlackListed = dto.IsBlackListed;
            user.BlackListReason = dto.BlackListReason;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "User updated successfully." });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "User deleted successfully." });
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

            return Ok(new { message = "Password changed successfully." });
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null)
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
                Address = user.Address,
                LicenseClass = user.LicenseClass,
                LicenseIssueDate = user.LicenseIssueDate,
                IsBlackListed = user.IsBlackListed,
                BlackListReason = user.BlackListReason,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                Roles = roles.ToList()
            };

            return Ok(dto);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("roles/assign")]
        public async Task<IActionResult> AssignRole([FromBody] RoleAssignDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            if (!await _roleManager.RoleExistsAsync(dto.RoleName))
                return BadRequest(new { message = "Role does not exist." });

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            var result = await _userManager.AddToRoleAsync(user, dto.RoleName);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Role assigned successfully." });
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
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim("FullName", user.FullName)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
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
