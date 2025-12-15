using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using RSD_E_Learning.Models;
using System.Text;

namespace RSD_E_Learning.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminTeacherController : Controller
    {
        private readonly DB _db;

        public AdminTeacherController(DB db)
        {
            _db = db;
        }

        // -------------------- LIST TEACHERS --------------------
        public async Task<IActionResult> Index()
        {
            var teachers = await _db.Teachers
                .Include(t => t.User)
                .OrderBy(t => t.User!.FullName)
                .ToListAsync();

            return View(teachers);
        }

        // -------------------- CREATE (GET) --------------------
        [HttpGet]
        public IActionResult Create()
        {
            return View(new TeacherCreateVm());
        }

        // -------------------- CREATE (POST) --------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TeacherCreateVm vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // Check duplicate email
            if (await _db.Users.AnyAsync(u => u.Email == vm.Email))
            {
                ModelState.AddModelError("Email", "Email already exists.");
                return View(vm);
            }

            // Create User (Teacher)
            var user = new DB.User
            {
                FullName = vm.FullName,
                Email = vm.Email,
                PasswordHash = HashPassword(vm.Password),
                Role = DB.UserRole.Teacher,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Create Teacher profile
            var teacher = new DB.Teacher
            {
                UserId = user.Id,
                SubjectArea = vm.SubjectArea,
                IsActive = true
            };

            _db.Teachers.Add(teacher);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Teacher account created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // -------------------- ACTIVATE / DEACTIVATE --------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var teacher = await _db.Teachers.FindAsync(id);
            if (teacher == null)
                return NotFound();

            teacher.IsActive = !teacher.IsActive;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // -------------------- PASSWORD HASHING (PBKDF2) --------------------
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

        // -------------------- RESET PASSWORD (GET) --------------------
        [HttpGet]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var teacher = await _db.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TeacherId == id);

            if (teacher == null)
                return NotFound();

            ViewBag.TeacherName = teacher.User!.FullName;
            return View(new ResetTeacherPasswordVm { TeacherId = id });
        }

        // -------------------- RESET PASSWORD (POST) --------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetTeacherPasswordVm vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var teacher = await _db.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TeacherId == vm.TeacherId);

            if (teacher == null)
                return NotFound();

            teacher.User!.PasswordHash = HashPassword(vm.NewPassword);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Teacher password has been reset successfully.";
            return RedirectToAction(nameof(Index));
        }

    }
}
