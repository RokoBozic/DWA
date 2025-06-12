using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    // ShopController: Handles product listing, filtering, and ordering for users.
    [Authorize(Roles = "User")]
    public class ShopController : Controller
    {
        private readonly EcommerceDbContext _context;
        private const int PageSize = 10;

        public ShopController(EcommerceDbContext context)
        {
            _context = context;
        }

        // Index: Displays a paginated list of products, with search and country filtering.
        public async Task<IActionResult> Index(string? searchTerm, int? countryId, int page = 1)
        {
            var query = _context.Products.Include(p => p.Country).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(p => p.Name.Contains(searchTerm));

            if (countryId.HasValue)
                query = query.Where(p => p.CountryId == countryId.Value);

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            var products = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    CountryName = p.Country.Name
                })
                .ToListAsync();

            var viewModel = new ProductListViewModel
            {
                Products = products,
                Countries = await _context.Countries.ToListAsync(),
                Page = page,
                TotalPages = totalPages,
                SearchTerm = searchTerm,
                CountryId = countryId
            };

            return View(viewModel);
        }

        // FilterAjax: Returns a partial view of filtered products for AJAX requests.
        [HttpGet]
        public async Task<IActionResult> FilterAjax(string? searchTerm, int? countryId, int page = 1)
        {
            var query = _context.Products.Include(p => p.Country).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(p => p.Name.Contains(searchTerm));

            if (countryId.HasValue)
                query = query.Where(p => p.CountryId == countryId.Value);

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            var products = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    CountryName = p.Country.Name
                })
                .ToListAsync();

            var viewModel = new ProductListViewModel
            {
                Products = products,
                Countries = await _context.Countries.ToListAsync(),
                Page = page,
                TotalPages = totalPages,
                SearchTerm = searchTerm,
                CountryId = countryId
            };

            return PartialView("_ProductListPartial", viewModel);
        }

        // Details: Displays detailed information for a specific product.
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products.Include(p => p.Country).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        // Order: Creates a new order for the current user and redirects to the product list.
        [HttpPost]
        public async Task<IActionResult> Order(int id)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Login", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (user == null || product == null) return RedirectToAction("Login", "Account");

            var order = new Order
            {
                UserId = user.Id,
                ProductId = product.Id,
                CountryId = product.CountryId ?? 0,
                Quantity = 1
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}

