using EAD.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EAD.Controllers
{
    public class Login : Controller
    {
        private readonly IConfiguration _config;
        private readonly EadProjectContext _context;  // ← Added: injected DbContext

        public Login(IConfiguration config, EadProjectContext context)  // ← Added context parameter
        {
            _config = config;
            _context = context;  // ← Store it for use in actions
        }

        [HttpGet]
        public IActionResult LoginPage()
        {
            // 1. Check if the JWT cookie exists
            string token = Request.Cookies["jwtToken"];
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    // 2. Decode the token to read its claims
                    var handler = new JwtSecurityTokenHandler();
                    // Check if token is actually a valid JWT format
                    if (handler.CanReadToken(token))
                    {
                        var jwtToken = handler.ReadJwtToken(token);
                        // 3. Extract the Role claim
                        var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role");

                        string userIdCookie = Request.Cookies["UserId"];
                        if (roleClaim != null)
                        {
                            string role = roleClaim.Value;
                            if (role == "Admin")
                            {
                                return RedirectToAction("AdminHome", "Dashboard");
                            }
                            else if (role == "User")
                            {
                                // If you still use the Cookie for ID, check it here
                                if (!string.IsNullOrEmpty(userIdCookie))
                                {
                                    return RedirectToAction("Home", "Dashboard");
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    ViewBag.Error = "Authentication Error";
                }
            }
            // If no token, or invalid token, show the view
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginPage(string email, string password, string role)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please fill all fields";
                return View();
            }

            try
            {
                if (role == "Admin")
                {
                    if (email == "admin@gmail.com" && password == "123")
                    {
                        var token = GenerateJwtToken(email, "Admin");
                        // Save JWT Token in HttpOnly Cookie
                        Response.Cookies.Append("jwtToken", token, new CookieOptions
                        {
                            HttpOnly = true,
                            //Secure = true, // only on https
                            Expires = DateTime.UtcNow.AddMinutes(60)
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
                    // ← CHANGED: Use injected _context instead of new EadProjectContext()
                    var usr = await _context.Users.FirstOrDefaultAsync(e => e.Email == email);

                    if (usr != null && BCrypt.Net.BCrypt.Verify(password, usr.Password))
                    {
                        if (usr.IsActive)
                        {
                            var token = GenerateJwtToken(email, "User");
                            // Save JWT Token in HttpOnly Cookie
                            Response.Cookies.Append("jwtToken", token, new CookieOptions
                            {
                                HttpOnly = true,
                                //Secure = true, // only on https
                                Expires = DateTime.UtcNow.AddMinutes(60)
                            });
                            Response.Cookies.Append("UserId", usr.Id.ToString(), new CookieOptions
                            {
                                Expires = DateTime.Now.AddDays(30),
                                HttpOnly = true,
                              //  Secure = true
                            });
                            return RedirectToAction("Home", "Dashboard");
                        }
                        else
                        {
                            ViewBag.Error = "Your account is currently deactivated, Contact admin";
                            return View();
                        }
                    }
                    else
                    {
                        ViewBag.Error = "Wrong credentials";
                        return View();
                    }
                }

                ViewBag.Error = "Server is not responding, Try Again";
            }
            catch (Exception)
            {
                ViewBag.Error = "Server error. Please try later.";
            }
            return View();
        }

        private string GenerateJwtToken(string username, string role)
        {
            var jwt = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwt["ExpireMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
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
            return RedirectToAction("LoginPage");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}