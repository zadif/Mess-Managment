using Microsoft.AspNetCore.Mvc;

namespace EAD.Controllers
{
    public class ErrorController : Controller
    {
       
        public IActionResult PageNotFound()
        {
            Response.StatusCode = 404;
            return View("404"); // Looks for Views/Shared/404.cshtml or Views/Error/404.cshtml
        }
    }
}
