using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using RSD_E_Learning.Models;
using System.Text;

namespace RSD_E_Learning.Controllers
{
    public class AdminTeacherController : Controller
    {
        private readonly DB _db;

        public AdminTeacherController(DB db)
        {
            _db = db;
        }

        // ================== LIST ==================
        public async Task<IActionResult> Index()
        {
            var teachers = await _db.Teachers
                .Include(t => t.User)
                .OrderBy(t => t.User!.FullName)
                .ToListAsync();

            return View(teachers);
        }

        // ================== CREATE (GET) ==================
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // ================== CREATE (POST) ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TeacherCreateVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (await _db.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email already exists.");
                return View(model);
            }

            var user = new DB.User
            {
                FullName = model.FullName,
                Email = model.Email,
                Role = DB.UserRole.Teacher,
                PasswordHash = HashPassword(model.Password),
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var teacher = new DB.Teacher
            {
                UserId = user.Id,
                SubjectArea = model.SubjectArea,
                IsActive = true
            };

            _db.Teachers.Add(teacher);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Teacher account created successfully.";
            return RedirectToAction("Index");
        }

        // ================== PASSWORD HASH ==================
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
    }
}
