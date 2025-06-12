using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    // ProfileController: Handles user profile management, accessible only to authenticated users.
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly EcommerceDbContext _context;

        public ProfileController(EcommerceDbContext context)
        {
            _context = context;
        }

        // Index: Displays the current user's profile information.
        public async Task<IActionResult> Index()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return NotFound();

            var viewModel = new UserProfileViewModel
            {
                Username = user.Username,
                Email = user.Email ?? ""
            };

            return View(viewModel);
        }

        // Update: Updates the user's profile information via AJAX.
        [HttpPost]
        public async Task<IActionResult> Update([FromBody] UserProfileViewModel model)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return NotFound();

            user.Email = model.Email;

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Profile updated successfully" });
        }
    }
}
