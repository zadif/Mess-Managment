using EAD.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EAD.Controllers
{
    [Authorize(AuthenticationSchemes = "JwtAuth", Roles = "User")]


    public class UserMenu : Controller
    {
        public async Task<IActionResult> fullMenu()
        {
            try
            {
                using (EadProjectContext db = new EadProjectContext())
                {
                    var temp = await db.DailyMenus.Include(e => e.MealItem).ToListAsync();
                    return View(temp);
                }
            }
            catch (Exception)
            {
                ViewBag.Error = "Server error. Please try later.";
                return View(new List<DailyMenu>()); // Return empty list so View doesn't crash
            }
        }

        public async Task<IActionResult> Bills()
        {
            try
            {
                string id = Request.Cookies["UserId"];

                // Basic Validation: Check if user is logged in
                if (string.IsNullOrEmpty(id) || !int.TryParse(id, out int userId))
                {
                    return RedirectToAction("LoginPage", "Account"); // Or wherever your login is
                }

                using (EadProjectContext db = new EadProjectContext())
                {
                    var temp = await db.Bills.Where(e => e.UserId == userId).ToListAsync();
                    var temp2 = await db.BillRecheckRequests.Where(e => e.UserId == userId).ToListAsync();

                    temp.Reverse();
                    temp2.Reverse();

                    BillViewModel b = new BillViewModel(temp, temp2);
                    return View(b);
                }
            }
            catch (Exception)
            {
                ViewBag.Error = "Server error. Please try later.";
                // Return empty ViewModel to prevent View crash
                return View(new BillViewModel(new List<Bill>(), new List<BillRecheckRequest>()));
            }
        }

        [HttpPost]
        public async Task<JsonResult> MarkAsPaid(int billId)
        {
            try
            {
                string idStr = Request.Cookies["UserId"];

                if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out int userId))
                {
                    return Json(0); // Error or Not Logged In
                }

                using (var db = new EadProjectContext())
                {
                    var bill = await db.Bills.Where(b => b.Id == billId && b.UserId == userId).Include(s=>s.DailyConsumptions).FirstOrDefaultAsync();

                    if (bill != null && !bill.IsPaid)
                    {
                        bill.IsPaid = true;
                        bill.PaidOn = DateTime.Now;

                        //Changing daily consumptions which are marked to recheck 
                        foreach (var consum in bill.DailyConsumptions)
                        {
                            if (consum.WasUserPresent == false)
                            {
                                consum.WasUserPresent = true;
                            }
                        }


                            await db.SaveChangesAsync(); // Changed to Async
                        return Json(1);
                    }
                }
            }
            catch (Exception)
            {
                // Log exception if needed
            }
            return Json(0);
        }

        [HttpPost]
        public async Task<IActionResult> RequestRecheck(int billId, string msg)
        {
            try
            {
                string idStr = Request.Cookies["UserId"];

                if (string.IsNullOrEmpty(msg))
                {
                    return Json(0); // Validation fail
                }

                if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out int userId))
                {
                    return Json(0);
                }

                using (var db = new EadProjectContext())
                {
                    var bill = await db.Bills.FirstOrDefaultAsync(b => b.Id == billId);

                    if (bill != null)
                    {
                        BillRecheckRequest bil = new BillRecheckRequest();
                        bil.UserId = userId;
                        bil.BillId = billId;
                        bil.RequestMessage = msg;
                        bil.Status = "Pending"; // Good practice to set default status

                        await db.BillRecheckRequests.AddAsync(bil);
                        await db.SaveChangesAsync();
                        return Json(1);
                    }
                }
            }
            catch (Exception)
            {
                // Log exception
            }
            return Json(0);
        }

        public async Task<IActionResult> RecheckBills()
        {
            try
            {
                string id = Request.Cookies["UserId"];

                if (string.IsNullOrEmpty(id) || !int.TryParse(id, out int userId))
                {
                    return RedirectToAction("LoginPage", "Account");
                }

                using (EadProjectContext db = new EadProjectContext())
                {
                    var temp2 = await db.BillRecheckRequests
                        .Where(e => e.UserId == userId)
                        .Include(s => s.Bill)
                        .ToListAsync();

                    temp2.Reverse();

                    return View(temp2);
                }
            }
            catch (Exception)
            {
                ViewBag.Error = "Server error. Please try later.";
                return View(new List<BillRecheckRequest>());
            }
        }

        public async Task<IActionResult> RecheckConsumptions()
        {
            try
            {
                string id = Request.Cookies["UserId"];

                if (string.IsNullOrEmpty(id) || !int.TryParse(id, out int userId))
                {
                    return RedirectToAction("LoginPage", "Account");
                }

                using (EadProjectContext db = new EadProjectContext())
                {
                    var temps = await db.DailyConsumptions
                        .Where(e => e.UserId == userId)
                        .Include(s => s.MealItem)
                        .Include(s => s.Bill)
                        .Select(m => new RecheckDailyConsumptionViewModel
                        {
                            Id = m.Id,
                            ConsumptionDate = m.ConsumptionDate,
                            WasUserPresent = m.WasUserPresent,
                            Quantity = m.Quantity,
                            IsBilled = m.IsBilled,
                            MealItem = m.MealItem,
                            Bill = m.Bill
                        })
                        .ToListAsync();

                    temps.Reverse();
                    return View(temps);
                }
            }
            catch (Exception)
            {
                ViewBag.Error = "Server error. Please try later.";
                return View(new List<RecheckDailyConsumptionViewModel>());
            }
        }

        [HttpPost]
        public async Task<JsonResult> RecheckConsumptions(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id) || !int.TryParse(id, out int consumptionId))
                {
                    return Json(0);
                }

                using (EadProjectContext db = new EadProjectContext())
                {
                    var temp = await db.DailyConsumptions.Where(e => e.Id == consumptionId).FirstOrDefaultAsync();
                    if (temp != null)
                    {
                        temp.WasUserPresent = false;
                        await db.SaveChangesAsync(); // Changed to Async
                        return Json(1);
                    }
                }
            }
            catch (Exception)
            {
                // Log error
            }
            return Json(0);
        }

        [HttpPost]
        public async Task<JsonResult> RecheckConsumptionsByBill(int billId)
        {
            try
            {
                using (EadProjectContext db = new EadProjectContext())
                {
                    var bill = await db.Bills
                        .Where(e => e.Id == billId)
                        .Include(b => b.DailyConsumptions)
                            .ThenInclude(dc => dc.MealItem)
                        .FirstOrDefaultAsync();

                    if (bill == null)
                        return Json(null);

                    var consumptions = bill.DailyConsumptions
                        .Select(dc => new
                        {
                            id = dc.Id,
                            consumptionDate = dc.ConsumptionDate.ToString("yyyy-MM-dd"),
                            wasUserPresent = dc.WasUserPresent,
                            mealItem = new
                            {
                                id = dc.MealItem.Id,
                                name = dc.MealItem.Name,
                                price = dc.MealItem.Price,
                                category = dc.MealItem.Category
                            },
                            quantity = dc.Quantity,
                            totalPrice = dc.Quantity * dc.MealItem.Price
                        })
                        .ToList();

                    return Json(consumptions);
                }
            }
            catch (Exception)
            {
                return Json(null);
            }
        }
    
    public async Task<IActionResult> Profile()
        {
                UserProfileViewModel cs = new UserProfileViewModel();
            try
            {
                string id = Request.Cookies["UserId"];
                using (EadProjectContext db = new EadProjectContext())
                {
                    var temp = await db.Users.Where(e => e.Id ==Convert.ToInt32( id)).FirstOrDefaultAsync();
                    cs.Name=temp.Name;
                   cs.Email= temp.Email;
                    cs.UserType = temp.UserType;
                    
                }
            }
            catch (Exception)
            {
                ViewBag.Error = "Server error. Please try later.";
               
            }
            return View(cs);
        }
    
        // 2. POST: Update Name and User Type
        [HttpPost]
        public async Task<IActionResult> UpdateInfo(UserProfileViewModel model)
        {
            string id = Request.Cookies["UserId"];

            try
            {
                using (EadProjectContext db = new EadProjectContext())
                {
                    var user = await db.Users.FirstOrDefaultAsync(u => u.Id == Convert.ToInt32(id));
                    if (user != null)
                    {
                        user.Name = model.Name;
                        user.UserType = model.UserType;

                        await db.SaveChangesAsync();
                        TempData["Success"] = "Profile updated successfully!";
                    }
                }
            }
            catch
            {
                TempData["Error"] = "Failed to update profile.";
            }
            return RedirectToAction("Profile");
        }

        // 3. POST: Change Password
        [HttpPost]
        public async Task<IActionResult> ChangePassword(UserProfileViewModel model)
        {
            string id = Request.Cookies["UserId"];

            try
            {
                using (EadProjectContext db = new EadProjectContext())
                {
                    var user = await db.Users.FirstOrDefaultAsync(u => u.Id == Convert.ToInt32(id));

                    // Check if user exists AND if the current password matches
                    if (user != null && BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.Password))
                    {
                        user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword); // Set new password
                        await db.SaveChangesAsync();
                        TempData["Success"] = "Password changed successfully!";
                    }
                    else
                    {
                        TempData["Error"] = "Incorrect current password. Please try again.";
                    }
                }
            }
            catch
            {
                TempData["Error"] = "An error occurred while changing password.";
            }
            return RedirectToAction("Profile");
        }

        // 4. POST: Deactivate Account
        [HttpPost]
        public async Task<IActionResult> DeactivateAccount()
        {
            string id = Request.Cookies["UserId"];
            
            try
            {
                using (EadProjectContext db = new EadProjectContext())
                {
                    var user = await db.Users.FirstOrDefaultAsync(u => u.Id == Convert.ToInt32(id));
                    if (user != null)
                    {
                       
                        user.IsActive = false;

                        await db.SaveChangesAsync();
                    }
                }

                // Log out the user explicitly
                Response.Cookies.Delete("UserId");

                TempData["Success"] = "Account deactivated. Goodbye!";
                return RedirectToAction("Logout", "Login"); 
            }
            catch
            {
                TempData["Error"] = "Could not deactivate account.";
                return RedirectToAction("Profile");
            }
        }
    }
}
