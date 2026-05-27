using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using TaskManagementApi.Data;
using TaskManagementApi.DTOs;
using TaskManagementApi.Models;

namespace TaskManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            {
                return BadRequest("Пользователь уже существует");
            }

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = HashPassword(dto.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Пользователь зарегистрирован ✅");
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
        
        [HttpPost("login")]
public async Task<IActionResult> Login(RegisterDto dto)
        {
    var user = await _context.Users
        .FirstOrDefaultAsync(u => u.Username == dto.Username);

    if (user == null || user.PasswordHash != HashPassword(dto.Password))
    {
        return Unauthorized("Неверный логин или пароль");
    }

    var token = GenerateJwtToken(user);

    return Ok(new { token });
       }
       private string GenerateJwtToken(User user)
       {
    var jwtSettings = HttpContext.RequestServices
        .GetRequiredService<IConfiguration>()
        .GetSection("Jwt");

    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(jwtSettings["Key"]));

    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username)
    };

    var token = new JwtSecurityToken(
        issuer: jwtSettings["Issuer"],
        audience: jwtSettings["Audience"],
        claims: claims,
        expires: DateTime.Now.AddMinutes(
            int.Parse(jwtSettings["ExpireMinutes"])),
        signingCredentials: creds);

    return new JwtSecurityTokenHandler().WriteToken(token);
       } 
    }
}
