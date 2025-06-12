using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;

namespace WebApp.Controllers
{
    // AdminController: Handles administrative actions, accessible only to administrators.
    public class AdminController : Controller
    {
        private readonly EcommerceDbContext _context;

        public AdminController(EcommerceDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/UserOrders
        // UserOrders: Displays a list of users and their orders, including product and country details.
        public async Task<IActionResult> UserOrders()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            var usersWithOrders = await _context.Users
                .Include(u => u.Orders)
                    .ThenInclude(o => o.Product)
                .Include(u => u.Orders)
                    .ThenInclude(o => o.Country)
                .ToListAsync();

            return View(usersWithOrders);
        }
    }
}
