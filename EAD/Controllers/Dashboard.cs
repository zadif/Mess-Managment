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
        public async Task<IActionResult> Home()
        {
            List<DailyMenuViewModel> list = new List<DailyMenuViewModel>();

            using (var db = new EadProjectContext())
            {
                // Get today's day (e.g., "Monday")
                string today = DateTime.Today.ToString("dddd");  // Returns "Monday", "Tuesday", etc.

                string id = Request.Cookies["UserId"];
                int userTypeInt;
                string userType="food";
                if (id != null)
                {
                    User usr = await db.Users.Where(e => e.Id == Convert.ToInt32(id)).FirstOrDefaultAsync();
                    if (usr != null)
                    {
                        userTypeInt = usr.UserType;
                        if (userTypeInt == 1) userType = "liquid";
                        else userType = "food";
                    }

                }
                if (userType == "liquid")
                {
                    list =await db.DailyMenus
                     .Include(m => m.MealItem)
                     .Where(d => d.DayOfWeek == today && (d.MealType=="Tea" || d.MealType == "Water"))
                     .Select(m => new DailyMenuViewModel
                     {
                         Id = m.Id,
                         DayOfWeek = m.DayOfWeek,
                         MealType = m.MealType,
                         MealItemName = m.MealItem != null ? m.MealItem.Name : "Not Set",
                         Price = m.MealItem != null ? m.MealItem.Price : 0,
                         Category = m.MealItem != null ? m.MealItem.Category : "",
                         Description = m.MealItem != null ? m.MealItem.Description : "",
                     })
                     .ToListAsync();
                }
                else
                {
                    list =await db.DailyMenus
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
                         Description = m.MealItem != null ? m.MealItem.Description : "",
                     })
                     .ToListAsync();
                }

                 
            }

            return View(list);  
        }
    }
}
