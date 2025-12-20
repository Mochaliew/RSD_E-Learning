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

        // ================= LIST =================
        public async Task<IActionResult> Index(string? search, bool? isActive, string? className)
        {
            var query = _db.Students
                .Include(s => s.User)
                .AsQueryable();

            // SEARCH (Name or Email)
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s =>
                    s.User!.FullName.Contains(search) ||
                    s.User.Email.Contains(search));
            }

            // FILTER: Active / Inactive
            if (isActive.HasValue)
            {
                if (isActive.Value)
                    query = query.Where(s => s.User!.LockoutEnd == null);
                else
                    query = query.Where(s => s.User!.LockoutEnd != null);
            }

            // FILTER: Class
            if (!string.IsNullOrWhiteSpace(className))
            {
                query = query.Where(s => s.ClassName == className);
            }

            var vm = new StudentFilterVm
            {
                Search = search,
                IsActive = isActive,
                ClassName = className,
                Students = await query
                    .OrderBy(s => s.User!.FullName)
                    .ToListAsync(),

                ClassNames = await _db.Students
                    .Where(s => s.ClassName != null)
                    .Select(s => s.ClassName!)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync()
            };

            return View(vm);
        }



        // ================= SINGLE TOGGLE (AJAX) =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatusAjax([FromBody] ToggleVm vm)
        {
            var student = await _db.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StudentId == vm.Id);

            if (student == null || student.User == null)
                return Json(new { success = false });

            bool isActive = student.User.LockoutEnd == null;

            student.User.LockoutEnd = isActive
                ? DateTime.UtcNow.AddYears(100)
                : null;

            _db.AuditLogs.Add(new DB.AuditLog
            {
                UserId = student.UserId,
                Action = isActive
                    ? $"Deactivated student account: {student.User.Email}"
                    : $"Activated student account: {student.User.Email}",
                Timestamp = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            return Json(new
            {
                success = true,
                isActive = student.User.LockoutEnd == null
            });
        }

        // ================= AJAX FILTER =================
        [HttpGet]
        public async Task<IActionResult> Filter(string? search, bool? isActive, string? className)
        {
            var query = _db.Students
                .Include(s => s.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s =>
                    s.User!.FullName.Contains(search) ||
                    s.User.Email.Contains(search));
            }

            if (isActive.HasValue)
            {
                query = isActive.Value
                    ? query.Where(s => s.User!.LockoutEnd == null)
                    : query.Where(s => s.User!.LockoutEnd != null);
            }

            if (!string.IsNullOrWhiteSpace(className))
            {
                query = query.Where(s => s.ClassName == className);
            }

            var students = await query
                .OrderBy(s => s.User!.FullName)
                .ToListAsync();

            return PartialView("_StudentTable", students);
        }


        // ================= BULK ACTIVATE / DEACTIVATE =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkUpdateStatus(int[] selectedIds, string actionType)
        {
            if (selectedIds == null || selectedIds.Length == 0)
                return RedirectToAction(nameof(Index));

            var students = await _db.Students
                .Include(s => s.User)
                .Where(s => selectedIds.Contains(s.StudentId))
                .ToListAsync();

            foreach (var student in students)
            {
                if (student.User == null) continue;

                if (actionType == "activate")
                {
                    student.User.LockoutEnd = null;

                    _db.AuditLogs.Add(new DB.AuditLog
                    {
                        UserId = student.UserId,
                        Action = $"Bulk activated student account: {student.User.Email}",
                        Timestamp = DateTime.UtcNow
                    });
                }
                else if (actionType == "deactivate")
                {
                    student.User.LockoutEnd = DateTime.UtcNow.AddYears(100);

                    _db.AuditLogs.Add(new DB.AuditLog
                    {
                        UserId = student.UserId,
                        Action = $"Bulk deactivated student account: {student.User.Email}",
                        Timestamp = DateTime.UtcNow
                    });
                }
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
