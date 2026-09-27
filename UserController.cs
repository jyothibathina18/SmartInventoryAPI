using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SmartInventoryAPI.Data;
using SmartInventoryAPI.DTOs;
using SmartInventoryAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartInventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _ctx;
        private readonly IConfiguration _config;

        public UserController(AppDbContext ctx, IConfiguration config)
        {
            _ctx = ctx;
            _config = config;
        }

       
        [HttpPost("register")]
        public IActionResult Register(RegisterDto dto)
        {
            var existing = _ctx.Users
                .FirstOrDefault(u => u.Email == dto.Email);

            if (existing != null)
            {
                return BadRequest("Email Already exists..");
            }

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = dto.Password,
                Role = "Customer"
            };

            _ctx.Users.Add(user);
            _ctx.SaveChanges();

            return Ok("Registration Successful...");
        }


        [HttpPost("login")]
        public IActionResult Login(LoginDto dto)
        {
            var user = _ctx.Users.FirstOrDefault(
                u => u.Email == dto.Email &&
                     u.Password == dto.Password);

            if (user == null)
            {
                return Unauthorized("Invalid Email or Password");
            }

            var claims = new[]
            {
                
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    user.Name),

                new Claim(
                    ClaimTypes.Email,
                    user.Email),

                new Claim(
                    ClaimTypes.Role,
                    user.Role)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _config["Jwt:Key"]!));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials
            );

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(token);

            return Ok(new
            {
                message = "Login Successful",
                token = jwt
            });
        }


        
        [Authorize]
        [HttpGet("profile")]
        public IActionResult Profile()
        {
            var userId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            var name = User.FindFirst(
                ClaimTypes.Name)?.Value;

            var email = User.FindFirst(
                ClaimTypes.Email)?.Value;

            var role = User.FindFirst(
                ClaimTypes.Role)?.Value;

            return Ok(new
            {
                userId,
                name,
                email,
                role
            });
        }
    }
}