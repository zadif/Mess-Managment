using EAD.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EAD.Controllers
{
    public class UserMenu : Controller
    {
        public IActionResult fullMenu()
        {
            using(EadProjectContext db= new EadProjectContext())
            {

                var temp = db.DailyMenus.Include(e => e.MealItem).ToList();
            return View(temp);
            }

        }
        public IActionResult Bills()
        {
            string id = Request.Cookies["UserId"];
            using (EadProjectContext db = new EadProjectContext())
            {

                var temp = db.Bills.Where(e => e.UserId == Convert.ToInt32(id)).ToList();
                return View(temp);
            }

        }
        [HttpPost]
        public JsonResult MarkAsPaid(int billId)
        {
            var userId = int.Parse(Request.Cookies["UserId"]);

            using (var db = new EadProjectContext())
            {

                var bill = db.Bills.FirstOrDefault(b => b.Id == billId && b.UserId == userId);

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
        public IActionResult RequestRecheck(int billId,string msg)
        {
            var userId = int.Parse(Request.Cookies["UserId"]);
           
            try
            {

                using (var db = new EadProjectContext())
                {
                var bill = db.Bills.FirstOrDefault(b => b.Id == billId);
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
    }
}
