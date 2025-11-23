using EAD.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EAD.Controllers
{
    public class Dashboard : Controller
    {
        public IActionResult AdminHome()
        {
            return View();
        }
        public IActionResult Home()
        {
            List<DailyMenuViewModel> list = new List<DailyMenuViewModel>();

            using (var db = new EadProjectContext())
            {
                // Get today's day (e.g., "Monday")
                string today = DateTime.Today.ToString("dddd");  // Returns "Monday", "Tuesday", etc.

                list = db.DailyMenus
                    .Include(m => m.MealItem)
                    .Where(d => d.DayOfWeek == today)
                    .Select(m => new DailyMenuViewModel
                    {
                        Id = m.Id,
                        DayOfWeek = m.DayOfWeek,
                        MealType = m.MealType,
                        MealItemName = m.MealItem != null ? m.MealItem.Name : "Not Set",
                        Price = m.MealItem != null ? m.MealItem.Price : 0,
                        Category = m.MealItem != null ? m.MealItem.Category : "",
                        Description=m.MealItem != null ?m.MealItem.Description: "",
                    })
                    .ToList();
            }

            return View(list);  
        }
    }
}
