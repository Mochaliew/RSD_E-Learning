using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using RSD_E_Learning.Models;
using Microsoft.IdentityModel.Tokens;

namespace RSD_E_Learning.Controllers
{
    public class AdminLoginController : Controller
    {
        private readonly DB _db;

        public AdminLoginController(DB db)
        {
            _db = db;
        }

        // --- GET: ADMIN LOGIN --- //
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // --- POST: ADMIN LOGIN --- //
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email && u.Role == DB.UserRole.Admin);

            if (user ==null || !VerifyPassword(password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid admin credentials.");
                return View();
            }

            var admin = await _db.Admins.FirstOrDefaultAsync(a => a.UserId == user.Id);
            if (admin == null)
            {
                ModelState.AddModelError("", "Admin account not found.");
                return View();
            }

            // --- CREATE COOKIE CLAIMS --- //
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("UserId", user.Id.ToString()),
                new Claim("AdminId", admin.AdminId.ToString()),
                new Claim("Role","Admin")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity)
                );

            return RedirectToAction("Index", "AdminDashboard");
        }

        // --- LOGOUT --- //
        public async Task<IActionResult> Logout()
        {
            await HttpContext .SignOutAsync();
            return RedirectToAction("Login");
        }

        // --- PASSWORD HASHING --- //
        private string HashPassword(string password)
        {
            byte[] salt = Encoding.UTF8.GetBytes("STATIC-SALT-CHANGE-LATER");

            return Convert.ToBase64String(
                KeyDerivation.Pbkdf2(
                    password,
                    salt,
                    KeyDerivationPrf.HMACSHA256,
                    10000,
                    32
                    )
                );

        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}
