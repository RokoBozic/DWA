using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;

namespace WebApp.Controllers
{
    // CountryController: Handles CRUD operations for countries.
    public class CountryController : Controller
    {
        private readonly EcommerceDbContext _context;

        public CountryController(EcommerceDbContext context)
        {
            _context = context;
        }

        // GET: Country
        // Index: Displays a list of all countries.
        public async Task<IActionResult> Index()
        {
            return View(await _context.Countries.ToListAsync());
        }

        // GET: Country/Create
        // Create: Displays the form to create a new country.
        public IActionResult Create()
        {
            return View();
        }

        // POST: Country/Create
        // Create (POST): Validates and saves a new country to the database.
        [HttpPost]
        public async Task<IActionResult> Create(Country country)
        {
            if (ModelState.IsValid)
            {
                _context.Countries.Add(country);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(country);
        }

        // GET: Country/Edit/5
        // Edit: Displays the form to edit an existing country.
        public async Task<IActionResult> Edit(int id)
        {
            var country = await _context.Countries.FindAsync(id);
            if (country == null) return NotFound();
            return View(country);
        }

        // POST: Country/Edit/5
        // Edit (POST): Validates and updates an existing country in the database.
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Country country)
        {
            if (id != country.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Entry(country).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(country);
        }

        // GET: Country/Delete/5
        // Delete: Displays the confirmation page for deleting a country.
        public async Task<IActionResult> Delete(int id)
        {
            var country = await _context.Countries.FindAsync(id);
            if (country == null) return NotFound();
            return View(country);
        }

        // POST: Country/Delete/5
        // DeleteConfirmed: Removes a country from the database, handling errors if the country is in use.
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var country = await _context.Countries.FindAsync(id);
            if (country != null)
            {
                try
                {
                    _context.Countries.Remove(country);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Country deleted successfully.";
                }
                catch (DbUpdateException ex)
                {
                    TempData["ErrorMessage"] = "Cannot delete this country because it is used by one or more products.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred while deleting the country. Please try again.";
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
