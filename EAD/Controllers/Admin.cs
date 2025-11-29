using EAD.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;

namespace EAD.wwwroot.js
{
    public class Admin : Controller
    {
        public IActionResult ManageUsers()
        {
            List<User> usr = new List<User>();
            using (EadProjectContext db = new EadProjectContext())
            {
                usr = db.Users.ToList();


            }
                return View(usr);
        }
        [HttpGet]
        public IActionResult AddUser()
        {
            return View(new User());
        }

        [HttpPost]
        public IActionResult AddUser(User user, string? NewPassword)
        {
            if (user.Id != 0)
            {
                ModelState.Remove("Password");
            }

            if (!ModelState.IsValid)
                return View(user);

            using (var db = new EadProjectContext())
            {
                if (user.Id == 0)
                {
                    // New User → Password is required
                    user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                    user.CreatedOn = DateTime.Now;
                    db.Users.Add(user);
                }
                else
                {
                    // Edit User → Update only if NewPassword is provided
                    var existing = db.Users.Find(user.Id);
                    if (existing != null)
                    {
                        existing.Name = user.Name;
                        existing.Email = user.Email;
                        existing.UserType = user.UserType;
                        existing.IsActive = user.IsActive;

                        // Only update password if user typed something
                        if (!string.IsNullOrWhiteSpace(NewPassword))
                        {
                            existing.Password = BCrypt.Net.BCrypt.HashPassword(NewPassword);
                        }
                    }
                }
                db.SaveChanges();
            }

            return RedirectToAction("ManageUsers");
        }
        public IActionResult EditUser(string id)
        {
            using (var db = new EadProjectContext())
            {
                var temp = db.Users.Where(usr => usr.Id == Convert.ToInt32(id)).FirstOrDefault();
                if (temp != null)
                {
                    return View("AddUser", temp);
                }
            }

                return RedirectToAction("ManageUsers");

        }
       
        public IActionResult DeleteUser(string id)
        {

            using (var db = new EadProjectContext())
            {
                var temp = db.Users.Where(usr => usr.Id == Convert.ToInt32(id)).FirstOrDefault();
                if (temp != null)
                {
                    db.Users.Remove(temp);
                    db.SaveChanges();
                }
            }

                return RedirectToAction("ManageUsers");

        }


        public IActionResult ManageMeals()
        {
            List<MealItem> items = new List<MealItem>();
            using (EadProjectContext db = new EadProjectContext())
            {
                items = db.MealItems.ToList();


            }
            return View(items);
        }
        [HttpGet]
        public IActionResult AddMeal()
        {
            return View(new MealItem());
        }

        [HttpPost]
        public IActionResult AddMeal(MealItem item)
        {
           
            if (!ModelState.IsValid)
                return View(item);

            using (var db = new EadProjectContext())
            {
                if (item.Id == 0)
                {
                    
                    db.MealItems.Add(item);
                }
                else
                {
                    // Edit User → Update only if NewPassword is provided
                    var existing = db.MealItems.Find(item.Id);
                    if (existing != null)
                    {
                        existing.Name = item.Name;
                        existing.Description = item.Description;
                        existing.Price = item.Price;
                        existing.Category = item.Category;

                      
                    }
                }
                db.SaveChanges();
            }

            return RedirectToAction("ManageMeals");
        }
        public IActionResult EditMeal(string id)
        {
            using (var db = new EadProjectContext())
            {
                var temp = db.MealItems.Where(usr => usr.Id == Convert.ToInt32(id)).FirstOrDefault();
                if (temp != null)
                {
                    return View("AddMeal", temp);
                }
            }

            return RedirectToAction("ManageMeals");

        }

        public IActionResult DeleteMeal(string id)
        {

            using (var db = new EadProjectContext())
            {
                var temp = db.MealItems.Where(usr => usr.Id == Convert.ToInt32(id)).FirstOrDefault();
                if (temp != null)
                {
                    db.MealItems.Remove(temp);
                    db.SaveChanges();
                }
            }

            return RedirectToAction("ManageMeals");

        }

        public IActionResult DailyMeals()
        {
            List<DailyMenuViewModel> meals = new List<DailyMenuViewModel>();
          
            using (EadProjectContext db = new EadProjectContext())
            {
               meals = db.DailyMenus
         .Include(m => m.MealItem)  
         .Select(m => new DailyMenuViewModel
         {
             Id = m.Id,
             DayOfWeek = m.DayOfWeek,
             MealType = m.MealType,
             MealItemName = m.MealItem != null ? m.MealItem.Name : "Not Assigned",
             Price = m.MealItem != null ? m.MealItem.Price : 0,
             Category = m.MealItem != null ? m.MealItem.Category : ""
         })
         .ToList();

            }
            return View(meals);
        }
        [HttpGet]
        public IActionResult SetDailyMeals()
        {
            List<MealItem> items = new List<MealItem>();
            using (EadProjectContext db = new EadProjectContext())
            {
                items = db.MealItems.ToList();


            }
            ViewBag.MealItems = items;
            return View(new DailyMenu());
        }
        [HttpPost]
        public IActionResult SaveDailyMenu(DailyMenu item)
        {

           
            //In menu.MealItem we are getting null , bcz we are loading it later
            ModelState.Remove("MealItem");

            
            if (!ModelState.IsValid)
            {
               
                return View("DailyMeals");
            }

            using (EadProjectContext db = new EadProjectContext())
            {
                if (item.Id == 0)
                db.DailyMenus.Add(item);
                else
                {
                    // Edit User → Update only if NewPassword is provided
                    var existing = db.DailyMenus.Find(item.Id);
                    if (existing != null)
                    {
                        existing.DayOfWeek = item.DayOfWeek;
                        existing.MealType = item.MealType;
                        existing.MealItemId = item.MealItemId;
                        existing.MealItem = item.MealItem;


                    }
                }

                db.SaveChanges();
            }

            return RedirectToAction("DailyMeals");
        }
        public IActionResult DeleteDailyMeal(string id)
        {

            using (var db = new EadProjectContext())
            {
                var temp = db.DailyMenus.Where(usr => usr.Id == Convert.ToInt32(id)).FirstOrDefault();
                if (temp != null)
                {
                    db.DailyMenus.Remove(temp);
                    db.SaveChanges();
                }
            }

            return RedirectToAction("DailyMeals");

        }


        public IActionResult EditDailyMeal(string id)
        {
            using (var db = new EadProjectContext())
            {
                var temp = db.DailyMenus.Where(usr => usr.Id == Convert.ToInt32(id)).FirstOrDefault();
                if (temp != null)
                {
                    List<MealItem> items = new List<MealItem>();
                 
                        items = db.MealItems.ToList();

                    ViewBag.MealItems = items;
                    return View("SetDailyMeals", temp);
                }
            }

            return RedirectToAction("ManageMeals");

        }


        public IActionResult generateDailyConsumptions()
        {
            string today = DateTime.Today.ToString("dddd");  // Returns "Monday", "Tuesday", etc.

            using (var db = new EadProjectContext())
            {
                var temp = db.DailyMenus.Include(s=>s.MealItem).Where(e=>e.DayOfWeek==today).ToList();
                if (temp != null)
                {
                    var users = db.Users.Where(e => e.IsActive == true).ToList();

                    if (users ==null) {
                        ViewBag.Error = "No User exists";
                        return View();
                    }
                   var today2 = DateOnly.FromDateTime(DateTime.Today);
                    var alreadyGeneratedUsers = db.DailyConsumptions
    .Where(d => d.ConsumptionDate == today2)
    .Select(d => d.UserId)
    .ToList();

                    ViewBag.AlreadyGenerated = alreadyGeneratedUsers;
                    ViewBag.Users = users;
                    ViewBag.Menus = temp;
                    return View();

                }
                else
                {
                    ViewBag.Error = "No Menu exists for today";
                    return View();
                }
            }

            return View();
        }

        [HttpPost]
        public IActionResult generateDailyConsumptions(string[] consumptions)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            using var db = new EadProjectContext();

            // Optional: Clear old records for today (or skip if you want to allow edits)
            //db.DailyConsumptions
            //    .Where(d => d.ConsumptionDate == today)
            //    .ExecuteDelete();

            if (consumptions != null && consumptions.Any())
            {
                foreach (var item in consumptions)
                {
                    var parts = item.Split('-');
                    if (parts.Length == 2 && int.TryParse(parts[0], out int userId) && int.TryParse(parts[1], out int mealItemId))
                    {
                        db.DailyConsumptions.Add(new DailyConsumption
                        {
                            UserId = userId,
                            MealItemId = mealItemId,
                            ConsumptionDate = today,
                            Quantity = 1
                        });
                    }
                }
            }

            db.SaveChanges();
            return RedirectToAction("GenerateDailyConsumptions");
        }

        [HttpGet]
        public JsonResult GetUserConsumption(int userId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            using (EadProjectContext db = new EadProjectContext())
            {
                // 1. Get what user has ALREADY consumed today
                var consumedItemIds = db.DailyConsumptions
                    .Where(d => d.UserId == userId && d.ConsumptionDate == today)
                    .Select(d => d.MealItemId)
                    .ToHashSet(); // HashSet is faster for lookups

                // 2. Get Today's Menu
                var menus = db.DailyMenus
                    .Include(m => m.MealItem)
                    .Where(m => m.DayOfWeek == today.DayOfWeek.ToString())
                    .ToList();

                var user = db.Users.Find(userId);

                // 3. Build a clean list of data objects
                var menuList = new List<object>();

                foreach (var menu in menus)
                {
                    // Logic: Is user allowed to consume this?
                    // UserType 2 = Food+Drinks. If UserType != 2, they can only see Non-Food items.
                    var canConsume = user.UserType == 2 ||
                                     !menu.MealItem.Category.Equals("Food", StringComparison.OrdinalIgnoreCase);

                    if (canConsume)
                    {
                        menuList.Add(new
                        {
                            menuId = menu.MealItemId,
                            mealType = menu.MealType,       // e.g., "Breakfast"
                            itemName = menu.MealItem.Name,  // e.g., "Omelette"
                            price = menu.MealItem.Price,    // Optional: if you want to show price
                            isChecked = consumedItemIds.Contains(menu.MealItemId) // true/false
                        });
                    }
                }

                // Return pure data
                return Json(new { userName = user.Name, userId = user.Id, items = menuList });
            }
        }
        [HttpPost]
        public IActionResult SaveUserConsumption(int userId, string[] consumptions)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            using (EadProjectContext db = new EadProjectContext())
                {

            db.DailyConsumptions
                .Where(d => d.UserId == userId && d.ConsumptionDate == today)
                .ExecuteDelete();

            if (consumptions != null)
            {
                foreach (var item in consumptions)
                {
                    var parts = item.Split('-');
                    if (int.TryParse(parts[1], out int mealItemId))
                    {
                        db.DailyConsumptions.Add(new DailyConsumption
                        {
                            UserId = userId,
                            MealItemId = mealItemId,
                            ConsumptionDate = today,
                            Quantity = 1
                        });
                    }
                }
                db.SaveChanges();
            }
            return Ok();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteTodayConsumption()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            using var db = new EadProjectContext();

            var deletedCount = db.DailyConsumptions
                .Where(d => d.ConsumptionDate == today && d.IsBilled == false)
                .ExecuteDelete();

            db.SaveChanges();

            if (deletedCount > 0)
                return Json(new { success = true, message = $"Deleted {deletedCount} consumption records for today." });
            else
                return Json(new { success = false, message = "No unbilled consumptions found for today." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUserConsumption(string userId)
        {
            if (!int.TryParse(userId, out int uid))
                return Json(new { success = false, message = "Invalid user ID." });

            var today = DateOnly.FromDateTime(DateTime.Today);

            using var db = new EadProjectContext();

            var deletedCount = db.DailyConsumptions
                .Where(d => d.UserId == uid && d.ConsumptionDate == today && d.IsBilled == false)
                .ExecuteDelete();

            db.SaveChanges();

            if (deletedCount > 0)
                return Json(new { success = true, message = "User's consumption deleted successfully." });
            else
                return Json(new { success = false, message = "No unbilled consumption found for this user today." });
        }

        public IActionResult Bills()
        {
            return View();
        }
   
        public IActionResult GenerateBills()
        {
            
            using(EadProjectContext db=new EadProjectContext())
            {
                var temps = db.DailyConsumptions.Where(e => e.IsBilled == false).Include(c => c.MealItem)
                       .Include(c => c.User).ToList();
            return View(temps);
            }


        }


        [HttpPost]
        public JsonResult GenerateBills(string userId, string total)
        {
            try
            {
                using (var db = new EadProjectContext())
                {
                    var bill = new Bill
                    {
                        UserId = Convert.ToInt32(userId),
                        TotalAmount = Convert.ToDecimal(total),
                    };
                    db.Bills.Add(bill);
                    db.SaveChanges();

                    // Mark consumptions as billed
                    var consumptions = db.DailyConsumptions
                        .Where(d => d.UserId == bill.UserId && d.IsBilled == false)
                        .ToList();

                    foreach (var c in consumptions)
                    {
                        c.IsBilled = true;
                        c.BillId = bill.Id;
                    }
                    db.SaveChanges();
                }

                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }


    }

}
