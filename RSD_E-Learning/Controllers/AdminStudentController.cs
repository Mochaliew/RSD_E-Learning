using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Models;

namespace RSD_E_Learning.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminStudentController : Controller
    {
        private readonly DB _db;

        public AdminStudentController(DB db)
        {
            _db = db;
        }

        // -------------------- MANAGE STUDENTS --------------------
        public async Task<IActionResult> Index()
        {
            var students = await _db.Students
                .Include(s => s.User)
                .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course)
                .OrderBy(s => s.User!.FullName)
                .ToListAsync();

            return View(students);
        }

        // -------------------- MANUAL DEACTIVATE (ADMIN ONLY) --------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAccountStatus(int studentId)
        {
            var student = await _db.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student == null)
                return NotFound();

            bool isActive =  student.User!.LockoutEnd == null;

            // Lock or unlock account
            student.User!.LockoutEnd =
                student.User.LockoutEnd == null
                    ? DateTime.UtcNow.AddYears(100)
                    : null;

            _db.AuditLogs.Add(new DB.AuditLog
            {
                Action = isActive
           ? $"Deactivated student account: {student.User.Email}"
           : $"Activated student account: {student.User.Email}",
                Timestamp = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ================= AJAX TOGGLE =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatusAjax([FromBody] ToggleVm vm)
        {
            var student = await _db.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StudentId == vm.Id);

            if (student == null || student.User == null)
                return Json(new { success = false });

            student.User.LockoutEnd =
                student.User.LockoutEnd == null
                    ? DateTime.UtcNow.AddYears(100)   // deactivate
                    : null;                           // activate

            _db.AuditLogs.Add(new DB.AuditLog
            {
                UserId = student.UserId,
                Action = student.User.LockoutEnd == null
                    ? $"Activated student account: {student.User.Email}"
                    : $"Deactivated student account: {student.User.Email}",
                Timestamp = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            return Json(new
            {
                success = true,
                isActive = student.User.LockoutEnd == null
            });
        }
    }
}
