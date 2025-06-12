using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;
using WebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.Controllers
{
    // OrderController: Handles displaying orders for users and administrators.
    public class OrderController : Controller
    {
        private readonly EcommerceDbContext _context;

        public OrderController(EcommerceDbContext context)
        {
            _context = context;
        }

        // Index: Displays a list of all orders, including user, product, and country details.
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Product)
                .Include(o => o.Country)
                .Select(o => new OrderViewModel
                {
                    Id = o.Id,
                    Username = o.User.Username,
                    ProductName = o.Product.Name,
                    CountryName = o.Country.Name,
                    Quantity = o.Quantity,
                    Price = o.Product.Price,
                    OrderDate = o.OrderDate
                })
                .ToListAsync();

            return View(orders);
        }

        // MyOrders: Displays a list of orders for the currently logged-in user.
        [Authorize]
        public async Task<IActionResult> MyOrders()
        {
            var username = User.Identity?.Name;
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Product)
                .Include(o => o.Country)
                .Where(o => o.User.Username == username)
                .Select(o => new OrderViewModel
                {
                    Id = o.Id,
                    Username = o.User.Username,
                    ProductName = o.Product.Name,
                    CountryName = o.Country.Name,
                    Quantity = o.Quantity,
                    Price = o.Product.Price,
                    OrderDate = o.OrderDate
                })
                .ToListAsync();

            return View("Index", orders);
        }
    }
} 