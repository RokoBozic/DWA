using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Models;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // ProductController: Handles CRUD operations for products, including search and paging.
    public class ProductController : ControllerBase
    {
        private readonly EcommerceDbContext _context;

        public ProductController(EcommerceDbContext context)
        {
            _context = context;
        }

        // GET: api/product
        [HttpGet]
        // GetAll: Retrieves all products from the database and logs the action.
        public async Task<IActionResult> GetAll()
        {
            var products = await _context.Products.Include(p => p.Country).ToListAsync();
            Log("INFO", "Retrieved all products.");
            return Ok(products);
        }

        // GET: api/product/5
        [HttpGet("{id}")]
        // Get: Retrieves a specific product by ID. Logs the action and returns a 404 if not found.
        public async Task<IActionResult> Get(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                Log("WARN", $"Product with ID={id} not found.");
                return NotFound();
            }

            Log("INFO", $"Retrieved product with ID={id}.");
            return Ok(product);
        }

        // POST: api/product
        [HttpPost]
        // Create: Adds a new product to the database. Logs the action and returns the created product.
        public async Task<IActionResult> Create([FromBody] Product product)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            Log("INFO", $"Product with ID={product.Id} created.");

            return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
        }

        // PUT: api/product/5
        [HttpPut("{id}")]
        // Update: Updates an existing product. Logs the action and returns a 404 if not found.
        public async Task<IActionResult> Update(int id, [FromBody] Product product)
        {
            if (id != product.Id) return BadRequest();

            _context.Entry(product).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
                Log("INFO", $"Product with ID={id} updated.");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Products.Any(p => p.Id == id))
                {
                    Log("ERROR", $"Product with ID={id} not found during update.");
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/product/5
        [HttpDelete("{id}")]
        // Delete: Removes a product from the database. Logs the action and returns a 404 if not found.
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                Log("WARN", $"Product with ID={id} not found during deletion.");
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            Log("INFO", $"Product with ID={id} deleted.");

            return NoContent();
        }

        // GET: api/product/search?name=x&page=1&count=10
        [HttpGet("search")]
        // Search: Searches for products by name, with paging support. Logs the search parameters.
        public async Task<IActionResult> Search(string? name, int page = 1, int count = 10)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(p => p.Name.Contains(name));
            }

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * count)
                .Take(count)
                .ToListAsync();

            Log("INFO", $"Searched products. Name={name}, Page={page}, Count={count}");
            return Ok(new { totalItems, items });
        }

        private void Log(string level, string message)
        {
            _context.Logs.Add(new Log
            {
                Level = level,
                Message = message
            });
            _context.SaveChanges();
        }
    }
}
