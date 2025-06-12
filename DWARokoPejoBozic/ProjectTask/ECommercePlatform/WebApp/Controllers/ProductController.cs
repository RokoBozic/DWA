using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;
using System.Diagnostics;
using System.Text.Json;
using System.Linq;

namespace WebApp.Controllers
{
    // ProductController: Handles CRUD operations for products, accessible only to administrators.
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly EcommerceDbContext _context;

        public ProductController(EcommerceDbContext context)
        {
            _context = context;
        }

        // GET: Product
        // Index: Displays a paginated list of products, with search and country filtering.
        public async Task<IActionResult> Index(string search, int? countryId, int page = 1)
        {
            int pageSize = 10;
            var products = _context.Products
                .Include(p => p.Country)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                products = products.Where(p => p.Name.Contains(search));

            if (countryId.HasValue)
                products = products.Where(p => p.CountryId == countryId.Value);

            var paged = await products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.HasNext = products.Count() > page * pageSize;
            ViewBag.Search = search;
            ViewBag.CountryId = countryId;
            ViewBag.Countries = await _context.Countries.ToListAsync();

            return View(paged);
        }

        // GET: Product/Create
        // Create: Displays the form to create a new product.
        public async Task<IActionResult> Create()
        {
            ViewBag.Countries = await _context.Countries.ToListAsync();
            return View(new WebApp.ViewModels.ProductCreateViewModel());
        }

        // POST: Product/Create
        // Create (POST): Validates and saves a new product to the database.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WebApp.ViewModels.ProductCreateViewModel model)
        {
            // Deep debug: log raw request body, ModelState, and CountryId
            try
            {
                Request.Body.Position = 0;
                using (var reader = new System.IO.StreamReader(Request.Body, System.Text.Encoding.UTF8, true, 1024, true))
                {
                    var body = await reader.ReadToEndAsync();
                    System.Diagnostics.Debug.WriteLine("RAW BODY: " + body);
                    Request.Body.Position = 0;
                }
            }
            catch { }
            System.Diagnostics.Debug.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            System.Diagnostics.Debug.WriteLine($"Model.CountryId: {model.CountryId}");

            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return Json(new { success = false, errors });
                }
                ViewBag.Countries = await _context.Countries.ToListAsync();
                return View(model);
            }

            var product = new Product
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                ImageUrl = model.ImageUrl,
                CountryId = model.CountryId ?? 0
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }

            TempData["SuccessMessage"] = "Product created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Product/Edit/5
        // Edit: Displays the form to edit an existing product.
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            var model = new WebApp.ViewModels.ProductEditViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                CountryId = product.CountryId
            };

            ViewBag.Countries = await _context.Countries.ToListAsync();
            return View(model);
        }

        // POST: Product/Edit/5
        // Edit (POST): Validates and updates an existing product in the database.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WebApp.ViewModels.ProductEditViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return Json(new { success = false, errors });
                }
                ViewBag.Countries = await _context.Countries.ToListAsync();
                return View(model);
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.Name = model.Name;
            product.Description = model.Description;
            product.Price = model.Price;
            product.ImageUrl = model.ImageUrl;
            product.CountryId = model.CountryId ?? 0;

            try
            {
                _context.Entry(product).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true });
                }
                TempData["SuccessMessage"] = "Product updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Products.AnyAsync(p => p.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, errors = new[] { "An error occurred while updating the product. Please try again." } });
                }
                ModelState.AddModelError("", "An error occurred while updating the product. Please try again.");
            }

            ViewBag.Countries = await _context.Countries.ToListAsync();
            return View(model);
        }

        // GET: Product/Delete/5
        // Delete: Displays the confirmation page for deleting a product.
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .Include(p => p.Country)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Product/Delete/5
        // DeleteConfirmed: Removes a product from the database.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product != null)
                {
                    _context.Products.Remove(product);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Product deleted successfully.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the product. Please try again.";
                // Log the exception here
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
