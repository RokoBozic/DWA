using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // AuthController: Handles user authentication, registration, and password changes.
    public class AuthController : ControllerBase
    {
        private readonly EcommerceDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(EcommerceDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        // Register: Registers a new user. Checks if the username already exists, hashes the password, and saves the user to the database.
        public IActionResult Register([FromBody] User user)
        {
            if (_context.Users.Any(u => u.Username == user.Username))
                return BadRequest("Username already exists.");

            // For demo: simple hash (NOT secure for production)
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            _context.Users.Add(user);
            _context.SaveChanges();

            // Add log entry for successful registration
            _context.Logs.Add(new Log
            {
                Level = "Info",
                Message = $"User {user.Username} registered.",
                Timestamp = DateTime.Now
            });
            _context.SaveChanges();

            return Ok("User registered successfully.");
        }

        [HttpPost("login")]
        // Login: Authenticates a user. Verifies the username and password, generates a JWT token, and returns it.
        public IActionResult Login([FromBody] User loginData)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == loginData.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginData.PasswordHash, user.PasswordHash))
            {
                // Add log entry for failed login
                _context.Logs.Add(new Log
                {
                    Level = "Warning",
                    Message = $"Failed login attempt for username: {loginData.Username}",
                    Timestamp = DateTime.Now
                });
                _context.SaveChanges();
                return Unauthorized("Invalid username or password.");
            }

            // Add log entry for successful login
            _context.Logs.Add(new Log
            {
                Level = "Info",
                Message = $"User {user.Username} logged in.",
                Timestamp = DateTime.Now
            });
            _context.SaveChanges();

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(new { token = tokenHandler.WriteToken(token) });
        }

        [HttpPost("changepassword")]
        // ChangePassword: Changes the user's password. Verifies the current password, hashes the new password, and updates the database.
        public IActionResult ChangePassword([FromBody] ChangePasswordModel model)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == model.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
                return Unauthorized("Invalid credentials.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            _context.SaveChanges();
            return Ok("Password changed successfully.");
        }

        public class ChangePasswordModel
        {
            public string Username { get; set; }
            public string CurrentPassword { get; set; }
            public string NewPassword { get; set; }
        }
    }
}
