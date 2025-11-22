using EAD.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace EAD.Controllers
{
    public class Login : Controller
    {
        public IActionResult LoginPage()
        {
            //checks if user is already logged in
            string role = Request.Cookies["Role"];
            if (role == "Admin")
            {
                return RedirectToAction("AdminHome", "Dashboard");

            }else if (role == "User")
            {
                string id = Request.Cookies["UserId"];
                if (id != null)
                {
                    return RedirectToAction("Home", "Dashboard");

                }
            }


            return View();
        }

        [HttpPost]
        public IActionResult LoginPage(string name, string password,string role)
        {
            if (role == "Admin")
            {
                if (name == "a" && password == "a")
                {
                    Response.Cookies.Append("Role", "Admin", new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(30),
                        HttpOnly = true,
                        Secure = true
                    });
                    return RedirectToAction("AdminHome", "Dashboard");
                }
                else
                {
                    ViewBag.Error = "Wrong credentials";
                    
                    return View();
                }
            }
            else
            {
                using(EadProjectContext db = new EadProjectContext())
                {
                    var usr = db.Users.FirstOrDefault(e => (e.Name == name || e.Email == name));
                    if (usr != null && BCrypt.Net.BCrypt.Verify(password, usr.Password))
                    {
                        // Save Role & UserId in cookies (lasts 30 days)
                        Response.Cookies.Append("Role", "User", new CookieOptions
                        {
                            Expires = DateTime.Now.AddDays(30),
                            HttpOnly = true,
                            Secure = true
                        });

                        Response.Cookies.Append("UserId",Convert.ToString( usr.Id), new CookieOptions
                        {
                            Expires = DateTime.Now.AddDays(30),
                            HttpOnly = true,
                            Secure = true
                        });
                       return RedirectToAction("Home", "Dashboard");

                        // Password is correct → login success
                    }
                    else
                    {
                        ViewBag.Error = "Wrong credentials";
                        return View();

                        // Wrong credentials
                    }
                }
            }
            ViewBag.Error = "Server is not responsing, Try Again";
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Delete ALL cookies (including custom ones)
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }
            return  RedirectToAction("LoginPage");
        }
    }
}
