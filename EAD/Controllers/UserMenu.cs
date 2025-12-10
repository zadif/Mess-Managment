using EAD.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EAD.Controllers
{
    public class UserMenu : Controller
    {
        public async Task< IActionResult> fullMenu()
        {
            using(EadProjectContext db= new EadProjectContext())
            {

                var temp = await db.DailyMenus.Include(e => e.MealItem).ToListAsync();
            return View(temp);
            }

        }
        public async Task<IActionResult> Bills()
        {
            string id = Request.Cookies["UserId"];
            using (EadProjectContext db = new EadProjectContext())
            {

                var temp =await db.Bills.Where(e => e.UserId == Convert.ToInt32(id)).ToListAsync();
                var temp2 =await  db.BillRecheckRequests.Where(e => e.UserId == Convert.ToInt32(id)).ToListAsync();

                BillViewModel b = new BillViewModel(temp,temp2);

                return View(b);
            }

        }
        [HttpPost]
        public async Task< JsonResult> MarkAsPaid(int billId)
        {
            var userId = int.Parse(Request.Cookies["UserId"]);

            using (var db = new EadProjectContext())
            {

                var bill =await db.Bills.FirstOrDefaultAsync(b => b.Id == billId && b.UserId == userId);

                if (bill != null && !bill.IsPaid)
                {
                    bill.IsPaid = true;
                    bill.PaidOn = DateTime.Now;
                    db.SaveChanges();
                    return Json(1);
                }
            }
            return Json(0);
        }

        [HttpPost]
        public async Task<IActionResult> RequestRecheck(int billId,string msg)
        {
            var userId = int.Parse(Request.Cookies["UserId"]);
           
            try
            {

                using (var db = new EadProjectContext())
                {
                var bill =await db.Bills.FirstOrDefaultAsync(b => b.Id == billId);
                    BillRecheckRequest bil = new BillRecheckRequest();
                    bil.UserId = userId;
                    bil.BillId = billId;
                    bil.RequestMessage = msg;

                    db.BillRecheckRequests.Add(bil);
                    db.SaveChanges();
                }
                return Json(1);
                
            }
            catch { }
            return Json(0);

        }

        public async Task<IActionResult> RecheckBills()
        {
            string id = Request.Cookies["UserId"];
            using (EadProjectContext db = new EadProjectContext())
            {

                var temp2 =await db.BillRecheckRequests.Where(e => e.UserId == Convert.ToInt32(id)).Include(s=>s.Bill).ToListAsync();


                return View(temp2);
            }
        }
    }
}
