using EAD.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;

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
            List<DailyMenu> meals = new List<DailyMenu>();
            using (EadProjectContext db = new EadProjectContext())
            {
                meals = db.DailyMenus.ToList();


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
        public IActionResult SaveDailyMenu(DailyMenu menu)
        {
            //In menu.MealItem we are getting null , bcz we are loading it later
            ModelState.Remove("MealItem");
            if (!ModelState.IsValid)
            {
               
                return View("DailyMeals");
            }

            using (EadProjectContext db = new EadProjectContext())
            {
                if (menu.Id == 0)
                db.DailyMenus.Add(menu);
            else
                db.DailyMenus.Update(menu);
            
                db.SaveChanges();
            }

            return RedirectToAction("DailyMeals");
        }
    }
}
