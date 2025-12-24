using EAD.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;
namespace EAD.Controllers
{
    public class Admin : Controller
    {
        public  async Task< IActionResult> ManageUsers()
        {
            List<User> usr = new List<User>();
            using (EadProjectContext db = new EadProjectContext())
            {
                usr = await db.Users.ToListAsync();


            }
                return View(usr);
        }
        [HttpGet]
        public  async Task< IActionResult> AddUser()
        {
            return View(new User());
        }

        [HttpPost]
        public  async Task< IActionResult> AddUser(User user, string? NewPassword)
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

                    // CHECK IF EMAIL ALREADY EXISTS
                    if (await db.Users.AnyAsync(u => u.Email == user.Email))
                    {
                    return    RedirectToAction("ManageUsers");
                    }
                    // New User → Password is required
                    user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                    user.CreatedOn = DateTime.Now;
                    await db.Users.AddAsync(user);
                }
                else
                {
                    // Edit User → Update only if NewPassword is provided
                    var existing = await db.Users.FindAsync(user.Id);
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
                await db.SaveChangesAsync();
            }

            return RedirectToAction("ManageUsers");
        }
        public  async Task< IActionResult> EditUser(string id)
        {
            using (var db = new EadProjectContext())
            {
                var temp = await db.Users.Where(usr => usr.Id == Convert.ToInt32(id)).FirstOrDefaultAsync();
                if (temp != null)
                {
                    return View("AddUser", temp);
                }
            }

                return RedirectToAction("ManageUsers");

        }
       
        public  async Task< IActionResult> DeleteUser(string id)
        {

            using (var db = new EadProjectContext())
            {
               int rowsAffected = await db.Users
        .Where(u => u.Id == Convert.ToInt32(id))
        .ExecuteDeleteAsync();
                await db.SaveChangesAsync();


            }

            return RedirectToAction("ManageUsers");

        }


        public  async Task< IActionResult> ManageMeals()
        {
            List<MealItem> items = new List<MealItem>();
            using (EadProjectContext db = new EadProjectContext())
            {
                items = await db.MealItems.ToListAsync();


            }
            return View(items);
        }
        [HttpGet]
        public  async Task< IActionResult> AddMeal()
        {
            return View(new MealItem());
        }

        [HttpPost]
        public  async Task< IActionResult> AddMeal(MealItem item)
        {
           
            if (!ModelState.IsValid)
                return View(item);

            using (var db = new EadProjectContext())
            {
                if (item.Id == 0)
                {
                    
                    await db.MealItems.AddAsync(item);
                }
                else
                {
                    // Edit User → Update only if NewPassword is provided
                    var existing = await db.MealItems.FindAsync(item.Id);
                    if (existing != null)
                    {
                        existing.Name = item.Name;
                        existing.Description = item.Description;
                        existing.Price = item.Price;
                        existing.Category = item.Category;

                      
                    }
                }
                await db.SaveChangesAsync();
            }

            return RedirectToAction("ManageMeals");
        }
        public  async Task< IActionResult> EditMeal(string id)
        {
            using (var db = new EadProjectContext())
            {
                var temp = await db.MealItems.Where(usr => usr.Id == Convert.ToInt32(id)).FirstOrDefaultAsync();
                if (temp != null)
                {
                    return View("AddMeal", temp);
                }
            }

            return RedirectToAction("ManageMeals");

        }

        public  async Task< IActionResult> DeleteMeal(string id)
        {

            using (var db = new EadProjectContext())
            {
                int rowsAffected = await db.MealItems
        .Where(u => u.Id == Convert.ToInt32(id))
        .ExecuteDeleteAsync();
                await db.SaveChangesAsync();

            }

            return RedirectToAction("ManageMeals");

        }

        public  async Task< IActionResult> DailyMeals()
        {
            List<DailyMenuViewModel> meals = new List<DailyMenuViewModel>();
          
            using (EadProjectContext db = new EadProjectContext())
            {
               meals = await db.DailyMenus
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
         .ToListAsync();

            }
            return View(meals);
        }
        [HttpGet]
        public  async Task< IActionResult> SetDailyMeals()
        {
            List<MealItem> items = new List<MealItem>();
            using (EadProjectContext db = new EadProjectContext())
            {
                items = await db.MealItems.ToListAsync();


            }
            ViewBag.MealItems = items;
            return View(new DailyMenu());
        }
        [HttpPost]
        public  async Task< IActionResult> SaveDailyMenu(DailyMenu item)
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
                await db.DailyMenus.AddAsync(item);
                else
                {
                    // Edit User → Update only if NewPassword is provided
                    var existing = await db.DailyMenus.FindAsync(item.Id);
                    if (existing != null)
                    {
                        existing.DayOfWeek = item.DayOfWeek;
                        existing.MealType = item.MealType;
                        existing.MealItemId = item.MealItemId;
                        existing.MealItem = item.MealItem;


                    }
                }

                await db.SaveChangesAsync();
            }

            return RedirectToAction("DailyMeals");
        }
        public  async Task< IActionResult> DeleteDailyMeal(string id)
        {

            using (var db = new EadProjectContext())
            {
                    int rowsAffected = await db.DailyMenus
        .Where(u => u.Id == Convert.ToInt32(id))
        .ExecuteDeleteAsync();
                    await db.SaveChangesAsync();
                
            }

            return RedirectToAction("DailyMeals");

        }


        public  async Task< IActionResult> EditDailyMeal(string id)
        {
            using (var db = new EadProjectContext())
            {
                var temp = await db.DailyMenus.Where(usr => usr.Id == Convert.ToInt32(id)).FirstOrDefaultAsync();
                if (temp != null)
                {
                    List<MealItem> items = new List<MealItem>();
                 
                        items = await db.MealItems.ToListAsync();

                    ViewBag.MealItems = items;
                    return View("SetDailyMeals", temp);
                }
            }

            return RedirectToAction("ManageMeals");

        }


        public  async Task< IActionResult> generateDailyConsumptions()
        {
            string today = DateTime.Today.ToString("dddd");  // Returns "Monday", "Tuesday", etc.

            using (var db = new EadProjectContext())
            {
                var temp = await db.DailyMenus.Include(s=>s.MealItem).Where(e=>e.DayOfWeek==today).ToListAsync();
                if (temp != null)
                {
                    var users = await db.Users.Where(e => e.IsActive == true).ToListAsync();

                    if (users ==null) {
                        ViewBag.Error = "No User exists";
                        return View();
                    }
                   var today2 = DateOnly.FromDateTime(DateTime.Today);
                    var alreadyGeneratedUsers = await db.DailyConsumptions
    .Where(d => d.ConsumptionDate == today2)
    .Select(d => d.UserId)
    .ToListAsync();

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
        public  async Task< IActionResult> generateDailyConsumptions(string[] consumptions)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            using var db = new EadProjectContext();

            // Optional: Clear old records for today (or skip if you want to allow edits)
            //await db.DailyConsumptions
            //    .Where(d => d.ConsumptionDate == today)
            //    .ExecuteDelete();

            if (consumptions != null && consumptions.Any())
            {
                foreach (var item in consumptions)
                {
                    var parts = item.Split('-');
                    if (parts.Length == 2 && int.TryParse(parts[0], out int userId) && int.TryParse(parts[1], out int mealItemId))
                    {
                        await db.DailyConsumptions.AddAsync(new DailyConsumption
                        {
                            UserId = userId,
                            MealItemId = mealItemId,
                            ConsumptionDate = today,
                            Quantity = 1,
                            WasUserPresent=true
                        });
                    }
                }
            }

            await db.SaveChangesAsync();
            return RedirectToAction("GenerateDailyConsumptions");
        }

        [HttpGet]
        public async Task<JsonResult> GetUserConsumption(int userId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            using (EadProjectContext db = new EadProjectContext())
            {
                // 1. Get what user has ALREADY consumed today
                var consumedItemIdList = await db.DailyConsumptions
             .Where(d => d.UserId == userId && d.ConsumptionDate == today)
             .Select(d => d.MealItemId)
             .ToListAsync(); 

                // 2. Convert to HashSet in memory (fast and efficient for lookups)
                var consumedItemIds = consumedItemIdList.ToHashSet();

                // 2. Get Today's Menu
                var menus = await db.DailyMenus
                    .Include(m => m.MealItem)
                    .Where(m => m.DayOfWeek == today.DayOfWeek.ToString())
                    .ToListAsync();

                var user = await db.Users.Where(u=>u.Id==userId).FirstOrDefaultAsync();

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
        public  async Task< IActionResult> SaveUserConsumption(int userId, string[] consumptions)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            using (EadProjectContext db = new EadProjectContext())
                {

            await db.DailyConsumptions
                .Where(d => d.UserId == userId && d.ConsumptionDate == today)
                .ExecuteDeleteAsync();

            if (consumptions != null)
            {
                foreach (var item in consumptions)
                {
                    var parts = item.Split('-');
                    if (int.TryParse(parts[1], out int mealItemId))
                    {
                        await db.DailyConsumptions.AddAsync(new DailyConsumption
                        {
                            UserId = userId,
                            MealItemId = mealItemId,
                            ConsumptionDate = today,
                            Quantity = 1
                        });
                    }
                }
                await db.SaveChangesAsync();
            }
            return Ok();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public  async Task< IActionResult> DeleteTodayConsumption()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            using var db = new EadProjectContext();

            var deletedCount = await db.DailyConsumptions
                .Where(d => d.ConsumptionDate == today && d.IsBilled == false)
                .ExecuteDeleteAsync();

            await db.SaveChangesAsync();

            if (deletedCount > 0)
                return Json(new { success = true, message = $"Deleted {deletedCount} consumption records for today." });
            else
                return Json(new { success = false, message = "No unbilled consumptions found for today." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public  async Task< IActionResult> DeleteUserConsumption(string userId)
        {
            if (!int.TryParse(userId, out int uid))
                return Json(new { success = false, message = "Invalid user ID." });

            var today = DateOnly.FromDateTime(DateTime.Today);

            using var db = new EadProjectContext();

            var deletedCount = await db.DailyConsumptions
                .Where(d => d.UserId == uid && d.ConsumptionDate == today && d.IsBilled == false)
                .ExecuteDeleteAsync();

            await db.SaveChangesAsync();

            if (deletedCount > 0)
                return Json(new { success = true, message = "User's consumption deleted successfully." });
            else
                return Json(new { success = false, message = "No unbilled consumption found for this user today." });
        }

        public  async Task< IActionResult> Bills()
        {
            return View();
        }
   
        public  async Task< IActionResult> GenerateBills()
        {
            
            using(EadProjectContext db=new EadProjectContext())
            {
                var temps = await db.DailyConsumptions.Where(e => e.IsBilled == false).Include(c => c.MealItem)
                       .Include(c => c.User).ToListAsync();
                temps.Reverse();
                return View(temps);
            }


        }


        [HttpPost]
        public async Task<JsonResult> GenerateBills(string userId, string total)
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
                    await db.Bills.AddAsync(bill);
                    await db.SaveChangesAsync();

                    // Mark consumptions as billed
                    var consumptions = await db.DailyConsumptions
                        .Where(d => d.UserId == bill.UserId && d.IsBilled == false)
                        .ToListAsync();

                    foreach (var c in consumptions)
                    {
                        c.IsBilled = true;
                        c.BillId = bill.Id;
                    }
                    await db.SaveChangesAsync();
                }

                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        public  async Task< IActionResult> statusOfBills()
        {
            using (EadProjectContext db = new EadProjectContext())
            {
                var temps = await db.Bills
                       .Include(c => c.User).ToListAsync();
                temps.Reverse();


                return View(temps);
            }
        }
        [HttpPost]
        public async Task< JsonResult> VerifyBill(int billId)
        {
            try
            {
                using (EadProjectContext db = new EadProjectContext())
                {

                    var bill = await db.Bills.FirstOrDefaultAsync(b => b.Id == billId);

                    if (bill != null && bill.IsPaid && !bill.VerifiedByAdmin)
                {
                    bill.VerifiedByAdmin = true;
                    await db.SaveChangesAsync();
                    return Json(1); // Success
                }
                }
            }
            catch { }

            return Json(0); // Error
        }


        public  async Task< IActionResult> recheckBills()
        {
            using(EadProjectContext db=new EadProjectContext())
            {
                var temp = await db.BillRecheckRequests.Include(s=>s.Bill).Include(s=>s.User)   .ToListAsync();
                temp.Reverse();

                return View(temp);
            }

        }



        [HttpPost]
        public async Task<JsonResult> ResolveRecheckRequest(int requestId, string action, decimal? newAmount)
        {
            using var db = new EadProjectContext();
            var request = await db.BillRecheckRequests.Include(r => r.Bill).FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null) return Json(new { success = false });

            if (action == "approve" && newAmount.HasValue && newAmount > 0)
            {
                request.Bill.TotalAmount = newAmount.Value;
                request.Status = "Approved";
            }
            else if (action == "reject")
            {
                request.Status = "Rejected";
            }
           

            await db.SaveChangesAsync();
            return Json(new { success = true });
        }
    
    
    public async Task<IActionResult> recheckConsumptions()
        {

            using(EadProjectContext db=new EadProjectContext())
            {
                var temps = await db.DailyConsumptions.Where(e => e.WasUserPresent == false).Include(s => s.User)
                      .Select(m => new recheckDailyConsumptionAdminViewModel
                      {
                          Id = m.Id,
                          ConsumptionDate = m.ConsumptionDate,
                          WasUserPresent = m.WasUserPresent,
                          Quantity = m.Quantity,
                     BillId=m.BillId,
                     User=m.User

                      })

                    .ToListAsync();
                temps.Reverse();

            return View(temps);
            }



        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ApproveRecheck(int id)
        {
            using (EadProjectContext db = new EadProjectContext())
            {
                //Reducing the bill
                var temp = await db.DailyConsumptions.Where(e => e.Id == id).Include(s => s.Bill).Include(s => s.MealItem).FirstOrDefaultAsync();
                temp.Bill.TotalAmount -= temp.MealItem.Price;
                    await db.DailyConsumptions.Where(e => e.Id == id).ExecuteDeleteAsync();
                    await db.SaveChangesAsync();
                }
                return Json(new { success = true });

            


        }

        [HttpPost]
        public async Task<JsonResult> RejectRecheck(int id)
        {
            using (EadProjectContext db = new EadProjectContext())
            {
                var consumption =await db.DailyConsumptions.Where(e => e.Id == id).FirstOrDefaultAsync();
                if (consumption != null)
                {
                consumption.WasUserPresent = true; // Put it back
                }
                await db.SaveChangesAsync();
            }
            return Json(new { success = true });


         
        }
    }

}
