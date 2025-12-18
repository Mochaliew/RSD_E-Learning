
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using RSD_E_Learning.Models;


namespace RSD_E_Learning.Controllers
{
    public class AccountController : Controller
    {
        private readonly DB _db;

        public AccountController(DB db)
        {
            _db = db;
        }


        // -------------------- REGISTER --------------------
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(StudentRegisterVm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // check existing email
            var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email already registered.");
                return View(model);
            }

            // create user first
            var user = new DB.User
            {
                FullName = model.FullName,
                Email = model.Email,
                Role = DB.UserRole.Student,
                PasswordHash = HashPassword(model.Password),
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // create student profile linked to user
            var student = new DB.Student
            {
                UserId = user.Id,
                EnrollmentDate = DateTime.UtcNow
            };

            _db.Students.Add(student);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Registration successful! Please login.";
            return RedirectToAction("Login");
        }


        // -------------------- LOGIN --------------------
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email && u.Role == DB.UserRole.Student);

            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View();
            }

            var student = await _db.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (student == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View();
            }

            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.UtcNow)
            {
                ModelState.AddModelError(
                    "",
                    "Your account has been locked. Please contact administrator."
                );
                return View();
            }   

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("FullName", user.FullName),
                new Claim("UserId", user.Id.ToString()),
                new Claim("StudentId", student.StudentId.ToString()),
                //new Claim("Role", "Student")
                new Claim(ClaimTypes.Role, "Student")

            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity)
            );

            return RedirectToAction("Dashboard", "Student");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }



        // -------------------- LOGOUT --------------------
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }


        // -------------------- PASSWORD HASHING --------------------
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


