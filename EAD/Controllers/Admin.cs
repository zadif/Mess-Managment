using EAD.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;

namespace EAD.Controllers
{
    [Authorize(AuthenticationSchemes = "JwtAuth", Roles = "Admin")]

    public class Admin : Controller
    {
        private readonly EadProjectContext _context;  // ← Added: injected  _contextContext

        public Admin(EadProjectContext context)  // ← Added context parameter
        {
            _context = context;
        }
        public async Task<IActionResult> ManageUsers()
        {
            List<User> usr = new List<User>();
            try
            {

                usr = await _context.Users.ToListAsync();

            }
            catch (Exception)
            {
                ViewBag.Error = "Server error. Please try later.";
            }
            return View(usr);
        }

        [HttpGet]
        public async Task<IActionResult> AddUser()
        {
            try
            {
                return View(new User());
            }
            catch (Exception)
            {
                ViewBag.Error = "Server error. Please try later.";
                return RedirectToAction("ManageUsers");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(User user, string? NewPassword)
        {
            try
            {
                if (user.Id != 0)
                {
                    ModelState.Remove("Password");
                }

                if (!ModelState.IsValid)
                    return View(user);


                {
                    if (user.Id == 0)
                    {
                        // CHECK IF EMAIL ALREADY EXISTS
                        if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                        {
                            ViewBag.Error = "Email already exists.";
                            return View(user);
                        }
                        // New User → Password is required
                        if (string.IsNullOrEmpty(user.Password))
                        {
                            ViewBag.Error = "Password is required.";
                            return View(user);
                        }

                        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                        user.CreatedOn = DateTime.Now;
                        await _context.Users.AddAsync(user);
                    }
                    else
                    {
                        // Edit User → Update only if NewPassword is provided
                        var existing = await _context.Users.FindAsync(user.Id);
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
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction("ManageUsers");
            }
            catch (Exception)
            {
                ViewBag.Error = "Server error. Please try later.";
                return View(user);
            }
        }

        public async Task<IActionResult> EditUser(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id)) return RedirectToAction("ManageUsers");


                {
                    var temp = await _context.Users.Where(usr => usr.Id == Convert.ToInt32(id)).FirstOrDefaultAsync();
                    if (temp != null)
                    {
                        return View("AddUser", temp);
                    }
                }
                return RedirectToAction("ManageUsers");
            }
            catch (Exception)
            {
                ViewBag.Error = "Server error. Please try later.";
                return RedirectToAction("ManageUsers");
            }
        }

        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id)) return RedirectToAction("ManageUsers");


                {
                    int rowsAffected = await _context.Users
                        .Where(u => u.Id == Convert.ToInt32(id))
                        .ExecuteDeleteAsync();
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction("ManageUsers");
            }
            catch (Exception)
            {
                // In a redirect scenario, ViewBag won't persist without TempData, 
                // but we prevent the crash here.
                return RedirectToAction("ManageUsers");
            }
        }

        public async Task<IActionResult> ManageMeals()
        {
            List<MealItem> items = new List<MealItem>();
            try
            {

                {
                    items = await _context.MealItems.ToListAsync();
                }
            }
            catch (Exception)
            {
                ViewBag.Error = "Server error. Please try later.";
            }
            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> AddMeal()
        {
            return View(new MealItem());
        }

        [HttpPost]
        public async Task<IActionResult> AddMeal(MealItem item)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(item);


                {
                    if (item.Id == 0)
                    {
                        await _context.MealItems.AddAsync(item);
                    }
                    else
                    {
                        var existing = await _context.MealItems.FindAsync(item.Id);
                        if (existing != null)
                        {
                            existing.Name = item.Name;
                            existing.Description = item.Description;
                            existing.Price = item.Price;
                            existing.Category = item.Category;
                        }
                    }
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction("ManageMeals");
            }
            catch (Exception)
            {
                ViewBag.Error = "Server error. Please try later.";
                return View(item);
            }
        }

        public async Task<IActionResult> EditMeal(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id)) return RedirectToAction("ManageMeals");


                {
                    var temp = await _context.MealItems.Where(usr => usr.Id == Convert.ToInt32(id)).FirstOrDefaultAsync();
                    if (temp != null)
                    {
                        return View("AddMeal", temp);
                    }
                }
                return RedirectToAction("ManageMeals");
            }
            catch (Exception)
            {
                return RedirectToAction("ManageMeals");
            }
        }

        public async Task<IActionResult> DeleteMeal(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id)) return RedirectToAction("ManageMeals");


                {
                    int rowsAffected = await _context.MealItems
                        .Where(u => u.Id == Convert.ToInt32(id))
                        .ExecuteDeleteAsync();
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction("ManageMeals");
            }
            catch (Exception)
            {
                return RedirectToAction("ManageMeals");
            }
        }

        public async Task<IActionResult> DailyMeals()
        {
            List<DailyMenuViewModel> meals = new List<DailyMenuViewModel>();
            try
            {

                {
                    meals = await _context.DailyMenus
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
            }
            catch (Exception)
            {
                ViewBag.Error = "Server error. Please try later.";
            }
            return View(meals);
        }

        [HttpGet]
        public async Task<IActionResult> SetDailyMeals()
        {
            try
            {
                List<MealItem> items = new List<MealItem>();

                {
                    items = await _context.MealItems.ToListAsync();
                }
                ViewBag.MealItems = items;
                return View(new DailyMenu());
            }
            catch (Exception)
            {
                ViewBag.Error = "Server error. Please try later.";
                return RedirectToAction("DailyMeals");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveDailyMenu(DailyMenu item)
        {
            try
            {
                ModelState.Remove("MealItem");

                if (!ModelState.IsValid)
                {
                    // Note: Returning "DailyMeals" view here might be an issue if it expects a List model
                    // but we will keep existing logic flow.
                    return View("DailyMeals");
                }


                {
                    if (item.Id == 0)
                        await _context.DailyMenus.AddAsync(item);
                    else
                    {
                        var existing = await _context.DailyMenus.FindAsync(item.Id);
                        if (existing != null)
                        {
                            existing.DayOfWeek = item.DayOfWeek;
                            existing.MealType = item.MealType;
                            existing.MealItemId = item.MealItemId;
                            existing.MealItem = item.MealItem;
                        }
                    }
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction("DailyMeals");
            }
            catch (Exception)
            {
                ViewBag.Error = "Server error. Please try later.";
                return RedirectToAction("DailyMeals");
            }
        }

        public async Task<IActionResult> DeleteDailyMeal(string id)
        {
            try
            {

                {
                    int rowsAffected = await _context.DailyMenus
                        .Where(u => u.Id == Convert.ToInt32(id))
                        .ExecuteDeleteAsync();
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction("DailyMeals");
            }
            catch (Exception)
            {
                return RedirectToAction("DailyMeals");
            }
        }

        public async Task<IActionResult> EditDailyMeal(string id)
        {
            try
            {

                {
                    var temp = await _context.DailyMenus.Where(usr => usr.Id == Convert.ToInt32(id)).FirstOrDefaultAsync();
                    if (temp != null)
                    {
                        List<MealItem> items = await _context.MealItems.ToListAsync();
                        ViewBag.MealItems = items;
                        return View("SetDailyMeals", temp);
                    }
                }
                return RedirectToAction("ManageMeals");
            }
            catch (Exception)
            {
                ViewBag.Error = "Server error. Please try later.";
                return RedirectToAction("DailyMeals");
            }
        }

        private DateOnly changeDateFormat(string date)
        {
            DateOnly Date;
            if (date == null)
            {

                Date = DateOnly.FromDateTime(DateTime.Today);
            }
            else
            {
                bool isValid = DateOnly.TryParseExact(
            date,
            "yyyy-MM-dd",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out Date);

                if (!isValid)
                {
                    Date = DateOnly.FromDateTime(DateTime.Today);
                    ViewBag.ErrorMessage = "Invalid date. Showing today.";
                }
            }
            return Date;
        }

        [HttpGet]
        public async Task<IActionResult> GetDailyConsumptionsData(string date = null)
        {
            try
            {
                string Date;
                DateTime targetDate;

                if (string.IsNullOrEmpty(date))
                {
                    targetDate = DateTime.Today;
                    Date = DateTime.Today.ToString("dddd");
                }
                else
                {
                    bool isValid = DateTime.TryParseExact(date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out targetDate);
                    if (isValid)
                    {
                        Date = targetDate.ToString("dddd");
                    }
                    else
                    {
                        targetDate = DateTime.Today;
                        Date = DateTime.Today.ToString("dddd");
                    }
                }

                var menus = await _context.DailyMenus.Include(s => s.MealItem).Where(e => e.DayOfWeek == Date).ToListAsync();
                var users = await _context.Users.Where(e => e.IsActive == true).ToListAsync();

                var today2 = DateOnly.FromDateTime(targetDate);
                var alreadyGeneratedUsers = await _context.DailyConsumptions
                    .Where(d => d.ConsumptionDate == today2)
                    .Select(d => d.UserId)
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    users = users.Select(u => new { u.Id, u.Name, u.UserType }),
                    menus = menus.Select(m => new { m.MealItemId, m.MealType, MealItem = new { m.MealItem.Name, m.MealItem.Price, m.MealItem.Category } }),
                    alreadyGenerated = alreadyGeneratedUsers,
                    displayDate = targetDate.ToString("dddd, MMMM dd, yyyy")
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Server error" });
            }
        }

        [HttpGet]
        public IActionResult generateDailyConsumptions()
        {
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetUserConsumption(int userId, string date)
        {
            try
            {
                DateOnly Date = changeDateFormat(date);


                {
                    var consumedItemIdList = await _context.DailyConsumptions
                         .Where(d => d.UserId == userId && d.ConsumptionDate == Date)
                         .Select(d => d.MealItemId)
                         .ToListAsync();

                    var consumedItemIds = consumedItemIdList.ToHashSet();

                    var menus = await _context.DailyMenus
                        .Include(m => m.MealItem)
                        .Where(m => m.DayOfWeek == Date.DayOfWeek.ToString())
                        .ToListAsync();

                    var user = await _context.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();

                    var menuList = new List<object>();

                    foreach (var menu in menus)
                    {
                        var canConsume = user.UserType == 2 ||
                                         !menu.MealItem.Category.Equals("Food", StringComparison.OrdinalIgnoreCase);

                        if (canConsume)
                        {
                            menuList.Add(new
                            {
                                menuId = menu.MealItemId,
                                mealType = menu.MealType,
                                itemName = menu.MealItem.Name,
                                price = menu.MealItem.Price,
                                isChecked = consumedItemIds.Contains(menu.MealItemId)
                            });
                        }
                    }
                    return Json(new { userName = user.Name, userId = user.Id, items = menuList });
                }
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Server error" });
            }
        }
        public class GenerateConsumptionRequest
        {
            public string[] Consumptions { get; set; }
            public string Date { get; set; }
        }
        [HttpPost]
        public async Task<IActionResult> GenerateDailyConsumptions(
   [FromForm] GenerateConsumptionRequest request)
        {
            try
            {
                string[] consumptions = request.Consumptions;
                string date = request.Date;
                DateOnly Date = changeDateFormat(date);

                int addedCount = 0;




                {
                    if (consumptions != null && consumptions.Any())
                    {
                        foreach (var item in consumptions)
                        {
                            var parts = item.Split('-');
                            if (parts.Length == 2 &&
                                int.TryParse(parts[0], out int userId) &&
                                int.TryParse(parts[1], out int mealItemId))
                            {
                                await _context.DailyConsumptions.AddAsync(new DailyConsumption
                                {
                                    UserId = userId,
                                    MealItemId = mealItemId,
                                    ConsumptionDate = Date,
                                    Quantity = 1,
                                    WasUserPresent = true
                                });
                                addedCount++;
                            }
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                return Json(new
                {
                    success = true,
                    message = $"Successfully added {addedCount} daily consumption records.",
                    addedCount = addedCount
                });
            }
            catch (Exception)
            {
                return Json(new
                {
                    success = false,
                    message = "Server error. Please try later."
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveUserConsumption(int userId, string[] consumptions, string date)
        {
            try
            {
                DateOnly Date = changeDateFormat(date);

                int addedCount = 0;


                {
                    // Delete existing records for this user today
                    await _context.DailyConsumptions
                        .Where(d => d.UserId == userId && d.ConsumptionDate == Date)
                        .ExecuteDeleteAsync();

                    if (consumptions != null && consumptions.Any())
                    {
                        foreach (var item in consumptions)
                        {
                            var parts = item.Split('-');
                            if (parts.Length >= 2 && int.TryParse(parts[1], out int mealItemId))
                            {
                                await _context.DailyConsumptions.AddAsync(new DailyConsumption
                                {
                                    UserId = userId,
                                    MealItemId = mealItemId,
                                    ConsumptionDate = Date,
                                    Quantity = 1
                                });
                                addedCount++;
                            }
                        }
                        await _context.SaveChangesAsync();
                    }

                    return Json(new
                    {
                        success = true,
                        message = "User consumption saved successfully.",
                        addedCount = addedCount
                    });
                }
            }
            catch (Exception)
            {
                return Json(new
                {
                    success = false,
                    message = "Server error."
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTodayConsumption(string date = null)
        {
            try
            {
                DateOnly Date = changeDateFormat(date);

                {
                    var deletedCount = await _context.DailyConsumptions
                        .Where(d => d.ConsumptionDate == Date && d.IsBilled == false)
                        .ExecuteDeleteAsync();

                    if (deletedCount > 0)
                    {
                        return Json(new
                        {
                            success = true,
                            message = $"Deleted {deletedCount} consumption records for today."
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            success = false,
                            message = "No unbilled consumptions found for today."
                        });
                    }
                }
            }
            catch (Exception)
            {
                return Json(new
                {
                    success = false,
                    message = "Server error."
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUserConsumption(string userId, string date)
        {
            try
            {
                if (!int.TryParse(userId, out int uid))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid user ID."
                    });
                }

                DateOnly Date = changeDateFormat(date);



                {
                    var deletedCount = await _context.DailyConsumptions
                        .Where(d => d.UserId == uid && d.ConsumptionDate == Date && d.IsBilled == false)
                        .ExecuteDeleteAsync();

                    if (deletedCount > 0)
                    {
                        return Json(new
                        {
                            success = true,
                            message = "User's consumption deleted successfully."
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            success = false,
                            message = "No unbilled consumption found for this user today."
                        });
                    }
                }
            }
            catch (Exception)
            {
                return Json(new
                {
                    success = false,
                    message = "Server error."
                });
            }
        }
        public async Task<IActionResult> Bills()
        {
            return View();
        }

        public async Task<IActionResult> GenerateBills()
        {
            try
            {

                {
                    var temps = await _context.DailyConsumptions.Where(e => e.IsBilled == false)
                        .Include(c => c.MealItem)
                        .Include(c => c.User).ToListAsync();
                    temps.Reverse();
                    return View(temps);
                }
            }
            catch (Exception)
            {
                ViewBag.Error = "Server error. Please try later.";
                return View(new List<DailyConsumption>());
            }
        }

        [HttpPost]
        public async Task<JsonResult> GenerateBills(string userId, string total)
        {
            try
            {

                {
                    var bill = new Bill
                    {
                        UserId = Convert.ToInt32(userId),
                        TotalAmount = Convert.ToDecimal(total),
                    };
                    await _context.Bills.AddAsync(bill);
                    await _context.SaveChangesAsync();

                    // Mark consumptions as billed
                    var consumptions = await _context.DailyConsumptions
                        .Where(d => d.UserId == bill.UserId && d.IsBilled == false)
                        .ToListAsync();

                    foreach (var c in consumptions)
                    {
                        c.IsBilled = true;
                        c.BillId = bill.Id;
                    }
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        public async Task<IActionResult> statusOfBills()
        {
            try
            {

                {
                    var temps = await _context.Bills
                           .Include(c => c.User).ToListAsync();
                    temps.Reverse();
                    return View(temps);
                }
            }
            catch (Exception)
            {
                ViewBag.Error = "Server error. Please try later.";
                return View(new List<Bill>());
            }
        }

        [HttpPost]
        public async Task<JsonResult> VerifyBill(int billId)
        {
            try
            {

                {
                    var bill = await _context.Bills.FirstOrDefaultAsync(b => b.Id == billId);

                    if (bill != null && bill.IsPaid && !bill.VerifiedByAdmin)
                    {
                        bill.VerifiedByAdmin = true;
                        await _context.SaveChangesAsync();
                        return Json(1); // Success
                    }
                }
            }
            catch { }

            return Json(0); // Error
        }

        public async Task<IActionResult> recheckBills()
        {
            try
            {

                {
                    var temp = await _context.BillRecheckRequests.Include(s => s.Bill).Include(s => s.User).ToListAsync();
                    temp.Reverse();
                    return View(temp);
                }
            }
            catch (Exception)
            {
                ViewBag.Error = "Server error. Please try later.";
                return View(new List<BillRecheckRequest>());
            }
        }

        [HttpPost]
        public async Task<JsonResult> ResolveRecheckRequest(int requestId, string action, decimal? newAmount)
        {
            try
            {

                {
                    var request = await _context.BillRecheckRequests.Include(r => r.Bill).FirstOrDefaultAsync(r => r.Id == requestId);

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

                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
            }
            catch (Exception)
            {
                return Json(new { success = false });
            }
        }

        public async Task<IActionResult> recheckConsumptions()
        {
            try
            {

                {
                    var temps = await _context.DailyConsumptions.Where(e => e.WasUserPresent == false).Include(s => s.User).Include(s => s.MealItem)
                          .Select(m => new recheckDailyConsumptionAdminViewModel
                          {
                              Id = m.Id,
                              ConsumptionDate = m.ConsumptionDate,
                              WasUserPresent = m.WasUserPresent,
                              Quantity = m.Quantity,
                              BillId = m.BillId,
                              User = m.User,
                              MealItem = m.MealItem
                          })
                        .ToListAsync();
                    temps.Reverse();

                    return View(temps);
                }
            }
            catch (Exception)
            {
                ViewBag.Error = "Server error. Please try later.";
                return View(new List<recheckDailyConsumptionAdminViewModel>());
            }
        }

        [HttpPost]
        public async Task<JsonResult> ApproveRecheck(int id)
        {
            try
            {

                {
                    //Reducing the bill
                    var temp = await _context.DailyConsumptions.Where(e => e.Id == id).Include(s => s.Bill).Include(s => s.MealItem).FirstOrDefaultAsync();
                    if (temp != null)
                    {
                        if (temp.Bill != null && temp.MealItem != null)
                        {
                            temp.Bill.TotalAmount -= temp.MealItem.Price;
                        }
                        await _context.DailyConsumptions.Where(e => e.Id == id).ExecuteDeleteAsync();
                        await _context.SaveChangesAsync();
                    }
                }
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false });
            }
        }

        [HttpPost]
        public async Task<JsonResult> RejectRecheck(int id)
        {
            try
            {

                {
                    var consumption = await _context.DailyConsumptions.Where(e => e.Id == id).FirstOrDefaultAsync();
                    if (consumption != null)
                    {
                        consumption.WasUserPresent = true; // Put it back
                    }
                    await _context.SaveChangesAsync();
                }
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false });
            }
        }

        public async Task<IActionResult> Statistics()
        {

            StatisticsViewModel cs = new StatisticsViewModel();

            try
            {
                {
                    cs.numUsers = await _context.Users.CountAsync();
                    cs.numInactiveUsers = await _context.Users.Where(e => e.IsActive == false).CountAsync();
                    cs.numBills = await _context.Bills.CountAsync();
                    var paidBills = await _context.Bills.Where(e => e.IsPaid == true).ToListAsync();
                    decimal total = 0;
                    foreach (var bil in paidBills)
                    {
                        total += bil.TotalAmount;
                    }
                    cs.total = total;
                    cs.numPaidBills = paidBills.Count();
                    cs.numMenuItems = await _context.MealItems.CountAsync();
                }
            }
            catch (Exception)
            {
                ViewBag.Error = "Server error. Please try later.";
            }
            return View(cs);


        }

    }
}