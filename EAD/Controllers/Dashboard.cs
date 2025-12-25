using EAD.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EAD.Controllers
{

    public class Dashboard : Controller
    {
        [Authorize(AuthenticationSchemes = "JwtAuth", Roles = "Admin")]

        public IActionResult AdminHome()
        {
            try
            {
                return View();
            }
            catch (Exception)
            {
                ViewBag.Error = "Server error. Please try later.";
                return View();
            }
        }
        [Authorize(AuthenticationSchemes = "JwtAuth", Roles = "User")]


        public async Task<IActionResult> Home()
        {
            List<DailyMenuViewModel> list = new List<DailyMenuViewModel>();

            try
            {
                using (var db = new EadProjectContext())
                {
                    // Get today's day (e.g., "Monday")
                    string today = DateTime.Today.ToString("dddd");  // Returns "Monday", "Tuesday", etc.

                    string id = Request.Cookies["UserId"];

                    // Default logic
                    string userType = "food";

                    // Validate ID before using it
                    if (!string.IsNullOrEmpty(id) && int.TryParse(id, out int userId))
                    {
                        User usr = await db.Users.Where(e => e.Id == userId).FirstOrDefaultAsync();
                        if (usr != null)
                        {
                            int userTypeInt = usr.UserType;
                            if (userTypeInt == 1) userType = "liquid";
                            else userType = "food";
                        }
                    }

                    if (userType == "liquid")
                    {
                        list = await db.DailyMenus
                         .Include(m => m.MealItem)
                         .Where(d => d.DayOfWeek == today && (d.MealType == "Tea" || d.MealType == "Water"))
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
                        list = await db.DailyMenus
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
            catch (Exception)
            {
                ViewBag.Error = "Server error. Please try later.";
                // Return empty list so the view does not crash
                return View(list);
            }
        }
    }
}