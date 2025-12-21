using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Models;
using RSD_E_Learning.Services;
using System.Text;

namespace RSD_E_Learning.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminTeacherController : Controller
    {
        private readonly DB _db;
        private readonly IEmailService _emailService; 

        public AdminTeacherController(DB db, IEmailService emailService)
        {
            _db = db;
            _emailService = emailService;
        }

        // ================== LIST ==================
        public async Task<IActionResult> Index(
    string? search,
    bool? isActive,
    string? subjectArea)
        {
            var query = _db.Teachers
                .Include(t => t.User)
                .AsQueryable();

            //  Search by Name or Email
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t =>
                    t.User!.FullName.Contains(search) ||
                    t.User.Email.Contains(search));
            }

            //  Filter by Status
            if (isActive.HasValue)
            {
                query = query.Where(t => t.IsActive == isActive.Value);
            }

            //  Filter by Subject Area
            if (!string.IsNullOrWhiteSpace(subjectArea))
            {
                query = query.Where(t => t.SubjectArea == subjectArea);
            }

            var vm = new TeacherFilterVm
            {
                Search = search,
                IsActive = isActive,
                SubjectArea = subjectArea,

                Teachers = await query
                    .OrderBy(t => t.User!.FullName)
                    .ToListAsync(),

                SubjectAreas = await _db.Teachers
                    .Select(t => t.SubjectArea)
                    .Distinct()
                    .OrderBy(s => s)
                    .ToListAsync()
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Filter(
            string? search,
            bool? isActive,
            string? subjectArea)
        {
            var query = _db.Teachers
                .Include(t => t.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t =>
                    t.User!.FullName.Contains(search) ||
                    t.User.Email.Contains(search));
            }

            if (isActive.HasValue)
            {
                query = query.Where(t => t.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(subjectArea))
            {
                query = query.Where(t => t.SubjectArea == subjectArea);
            }

            var teachers = await query
                .OrderBy(t => t.User!.FullName)
                .ToListAsync();

            return PartialView("_TeacherTable", teachers);
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

            _db.AuditLogs.Add(new DB.AuditLog
            {
                UserId = user.Id,
                Action = $"Created teacher account: {user.Email}",
                Timestamp = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            TempData["TeacherSuccess"] = "Teacher account created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ================== ACTIVATE / DEACTIVATE ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int teacherId)
        {
            var teacher = await _db.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TeacherId == teacherId);

            if (teacher == null || teacher.User == null)
                return NotFound();

            bool wasActive = teacher.IsActive;
            teacher.IsActive = !teacher.IsActive;

            _db.AuditLogs.Add(new DB.AuditLog
            {
                UserId = teacher.UserId,
                Action = wasActive
                    ? $"Deactivated teacher account: {teacher.User.Email}"
                    : $"Activated teacher account: {teacher.User.Email}",
                Timestamp = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ================== ACTIVATE / DEACTIVATE (AJAX) ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatusAjax([FromBody] ToggleVm vm)
        {
            var teacher = await _db.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TeacherId == vm.Id);

            if (teacher == null || teacher.User == null)
                return Json(new { success = false });

            bool wasActive = teacher.IsActive;
            teacher.IsActive = !teacher.IsActive;

            if (teacher.IsActive)
            {
                await _emailService.SendAsync(
                    teacher.User.Email,
                    "Teacher Account Activated",
                    EmailTemplates.TeacherActivated(teacher.User.FullName)
                );
            }
            else
            {
                await _emailService.SendAsync(
                    teacher.User.Email,
                    "Teacher Account Deactivated",
                    EmailTemplates.TeacherDeactivated(teacher.User.FullName)
                );
            }

            _db.AuditLogs.Add(new DB.AuditLog
            {
                UserId = teacher.UserId,
                Action = wasActive
                    ? $"Deactivated teacher account: {teacher.User.Email}"
                    : $"Activated teacher account: {teacher.User.Email}",
                Timestamp = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            return Json(new
            {
                success = true,
                isActive = teacher.IsActive
            });
        }

        // ================== RESET PASSWORD (GET) ==================
        [HttpGet]
        public async Task<IActionResult> ResetTeacherPassword(int teacherId)
        {
            var teacher = await _db.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TeacherId == teacherId);

            if (teacher == null || teacher.User == null)
                return NotFound();

            var vm = new ResetTeacherPasswordVm
            {
                UserId = teacher.UserId
            };

            return View("ResetPassword",vm);
        }

        // ================== RESET PASSWORD (POST) ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetTeacherPassword(ResetTeacherPasswordVm model)
        {
            if (!ModelState.IsValid)
                return View("ResetPassword",model);

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Id == model.UserId && u.Role == DB.UserRole.Teacher);

            if (user == null)
                return NotFound();

            user.PasswordHash = HashPassword(model.NewPassword);

            _db.AuditLogs.Add(new DB.AuditLog
            {
                UserId = user.Id,
                Action = $"Admin reset password for teacher: {user.Email}",
                Timestamp = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            TempData["Success"] = "Teacher password has been reset successfully.";
            return RedirectToAction(nameof(Index));
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
