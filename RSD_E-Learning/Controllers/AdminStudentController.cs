using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Models;
using RSD_E_Learning.Services;

namespace RSD_E_Learning.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminStudentController : Controller
    {
        private readonly DB _db;
        private readonly IEmailService _emailService;

        public AdminStudentController(DB db, IEmailService emailService )
        {
            _db = db;
            _emailService = emailService;
        }

        // ================= LIST =================
        public async Task<IActionResult> Index(string? search, bool? isActive)
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

            var vm = new StudentFilterVm
            {
                Search = search,
                IsActive = isActive,
                Students = await query
                    .OrderBy(s => s.User!.FullName)
                    .ToListAsync(),
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

            if (isActive)
            {
                // DEACTIVATE
                student.User.LockoutEnd = DateTime.UtcNow.AddYears(100);

                await _emailService.SendAsync(
                    student.User.Email,
                    "Account Deactivated",
                    EmailTemplates.StudentDeactivated(student.User.FullName)
                );
            }
            else
            {
                // ACTIVATE
                student.User.LockoutEnd = null;

                await _emailService.SendAsync(
                    student.User.Email,
                    "Account Activated",
                    EmailTemplates.StudentActivated(student.User.FullName)
                );
            }

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
        public async Task<IActionResult> Filter(string? search, bool? isActive)
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

                    await _emailService.SendAsync(
                        student.User.Email,
                        "Account Activated",
                        EmailTemplates.StudentActivated(student.User.FullName)
                    );

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

                    await _emailService.SendAsync(
                        student.User.Email,
                        "Account Deactivated",
                        EmailTemplates.StudentDeactivated(student.User.FullName)
                    );

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
