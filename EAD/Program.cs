using EAD.Controllers;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
  .AddCookie(options =>
  {
      options.LoginPath = "/Login/LoginPage";           
      options.AccessDeniedPath = "/Login/LoginPage";    
      options.ExpireTimeSpan = TimeSpan.FromDays(7);
      options.SlidingExpiration = true;
      options.Cookie.HttpOnly = true;   // Recommended security
      options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // or Always in production
  });
builder.Services.AddAuthorization();
// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=LoginPage}/{id?}");

app.Run();
