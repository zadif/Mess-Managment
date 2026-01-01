using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EAD.Models;

namespace EAD.Controllers
{
    public class DebugController : Controller
    {
        private readonly EadProjectContext _context;

        public DebugController(EadProjectContext context)
        {
            _context = context;
        }

        [HttpGet("/migrate-db")]
        public IActionResult Migrate()
        {
            try
            {
                _context.Database.Migrate();  // This creates all tables
                return Content("<h2>Success!</h2> Database tables created successfully.<br><br>Tables: Users, MealItems, Bills, DailyConsumptions, etc.<br><br>You can now delete this controller.");
            }
            catch (Exception ex)
            {
                return Content("<pre>Error: " + ex.Message + "\n\n" + ex.InnerException?.Message + "</pre>");
            }
        }
    }
}