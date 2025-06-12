using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Models;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // LogsController: Handles retrieving logs from the database.
    public class LogsController : ControllerBase
    {
        private readonly EcommerceDbContext _context;

        public LogsController(EcommerceDbContext context)
        {
            _context = context;
        }

        // GET: api/logs/get/10
        // GetLastNLogs: Retrieves the last N logs from the database, ordered by timestamp.
        [HttpGet("get/{n}")]
        public async Task<IActionResult> GetLastNLogs(int n)
        {
            var logs = await _context.Logs
                .OrderByDescending(log => log.Timestamp)
                .Take(n)
                .ToListAsync();

            return Ok(logs);
        }

        // GET: api/logs/count
        // GetLogCount: Returns the total number of logs in the database.
        [HttpGet("count")]
        public async Task<IActionResult> GetLogCount()
        {
            var count = await _context.Logs.CountAsync();
            return Ok(new { count });
        }
    }
}
