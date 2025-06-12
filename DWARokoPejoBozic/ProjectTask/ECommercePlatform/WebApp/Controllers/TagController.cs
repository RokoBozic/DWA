using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;

namespace WebApp.Controllers
{
    // TagController: Handles CRUD operations for tags.
    public class TagController : Controller
    {
        private readonly EcommerceDbContext _context;

        public TagController(EcommerceDbContext context)
        {
            _context = context;
        }

        // GET: Tag
        // Index: Displays a list of all tags.
        public async Task<IActionResult> Index()
        {
            return View(await _context.Tags.ToListAsync());
        }

        // GET: Tag/Create
        // Create: Displays the form to create a new tag.
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tag/Create
        // Create (POST): Validates and saves a new tag to the database.
        [HttpPost]
        public async Task<IActionResult> Create(Tag tag)
        {
            if (ModelState.IsValid)
            {
                _context.Tags.Add(tag);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tag);
        }

        // GET: Tag/Edit/5
        // Edit: Displays the form to edit an existing tag.
        public async Task<IActionResult> Edit(int id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null) return NotFound();
            return View(tag);
        }

        // POST: Tag/Edit/5
        // Edit (POST): Validates and updates an existing tag in the database.
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Tag tag)
        {
            if (id != tag.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Entry(tag).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tag);
        }

        // GET: Tag/Delete/5
        // Delete: Displays the confirmation page for deleting a tag.
        public async Task<IActionResult> Delete(int id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null) return NotFound();
            return View(tag);
        }

        // POST: Tag/Delete/5
        // DeleteConfirmed: Removes a tag from the database, handling errors if the tag is in use.
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag != null)
            {
                try
                {
                    _context.Tags.Remove(tag);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Tag deleted successfully.";
                }
                catch (DbUpdateException ex)
                {
                    TempData["ErrorMessage"] = "Cannot delete this tag because it is used by one or more products.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred while deleting the tag. Please try again.";
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
