using Microsoft.AspNetCore.Mvc;

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
            return View();
        }
    }
}
