using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using WebApp.Models;
using WebApp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Controllers
{
    // AccountController: Handles user registration, login, logout, and profile management.
    public class AccountController : Controller
    {
        private readonly EcommerceDbContext _context;

        public AccountController(EcommerceDbContext context)
        {
            _context = context;
        }

        // Register: Displays the registration form and processes new user registrations.
        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (_context.Users.Any(u => u.Username == user.Username))
            {
                ModelState.AddModelError("", "Username already exists.");
                return View(user);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            user.Role = "User";

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Add log entry for successful registration
            _context.Logs.Add(new Log
            {
                Level = "Info",
                Message = $"User {user.Username} registered.",
                Timestamp = DateTime.Now
            });
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        // Login: Displays the login form and authenticates users.
        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                // Add log entry for failed login
                _context.Logs.Add(new Log
                {
                    Level = "Warning",
                    Message = $"Failed login attempt for username: {username}",
                    Timestamp = DateTime.Now
                });
                await _context.SaveChangesAsync();
                ModelState.AddModelError("", "Invalid credentials.");
                return View();
            }

            // Add log entry for successful login
            _context.Logs.Add(new Log
            {
                Level = "Info",
                Message = $"User {user.Username} logged in.",
                Timestamp = DateTime.Now
            });
            await _context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Email, user.Email ?? "")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (user.Role == "Admin")
                return RedirectToAction("Index", "Product");
            else
                return RedirectToAction("Index", "Shop");
        }

        // Logout: Signs out the current user and redirects to the login page.
        // GET: /Account/Logout
        public async Task<IActionResult> Logout()
        {
            // Add log entry for logout
            _context.Logs.Add(new Log
            {
                Level = "Info",
                Message = $"User {User.Identity?.Name} logged out.",
                Timestamp = DateTime.Now
            });
            await _context.SaveChangesAsync();

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // Profile: Displays the current user's profile information.
        // GET: /Account/Profile
        public async Task<IActionResult> Profile()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Login");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return NotFound();

            var model = new UserProfileViewModel
            {
                Username = user.Username,
                Email = user.Email ?? ""
            };

            return View(model);
        }

        // UpdateProfile: Updates the user's profile information via AJAX.
        // POST: /Account/UpdateProfile
        [HttpPost]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileViewModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
            if (user == null) return NotFound();

            user.Email = model.Email;

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Profile updated successfully." });
        }
    }
}

