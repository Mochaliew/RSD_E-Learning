using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Models;

namespace RSD_E_Learning.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminCourseController : Controller
    {
        private readonly DB _db;

        public AdminCourseController(DB db)
        {
            _db = db;
        }

        // =========================
        // LIST + FILTER COURSES
        // =========================
        public async Task<IActionResult> Index(string? search, string filter = "all")
        {
            var query = _db.Courses
                .Include(c => c.Teacher).ThenInclude(t => t.User)
                .Include(c => c.Category)
                .AsQueryable();

            // SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    c.Title.Contains(search) ||
                    c.Teacher!.User!.FullName.Contains(search));
            }

            // FILTER
            switch (filter)
            {
                case "pending":
                    query = query.Where(c => !c.IsApproved && !c.IsRejected);
                    break;
                case "approved":
                    query = query.Where(c => c.IsApproved && !c.IsRejected);
                    break;
                case "rejected":
                    query = query.Where(c => c.IsRejected);
                    break;
            }

            var vm = new CourseFilterVm
            {
                Search = search,
                Filter = filter,
                Courses = await query
                    .OrderByDescending(c => c.CourseId)
                    .ToListAsync()
            };

            return View(vm);
        }

        // ================= AJAX SEARCH + FILTER =================
        [HttpGet]
        public async Task<IActionResult> AjaxList(string? search, string filter = "all")
        {
            var query = _db.Courses
                .Include(c => c.Teacher).ThenInclude(t => t.User)
                .Include(c => c.Category)
                .AsQueryable();

            // SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    c.Title.Contains(search) ||
                    c.Teacher!.User!.FullName.Contains(search));
            }

            // FILTER
            switch (filter)
            {
                case "pending":
                    query = query.Where(c => !c.IsApproved && !c.IsRejected);
                    break;
                case "approved":
                    query = query.Where(c => c.IsApproved && !c.IsRejected);
                    break;
                case "rejected":
                    query = query.Where(c => c.IsRejected);
                    break;
            }

            var courses = await query
                .OrderByDescending(c => c.CourseId)
                .ToListAsync();

            return PartialView("_CourseTable", courses);
        }


        // =========================
        // PUBLISH / UNPUBLISH
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePublish(int id)
        {
            var course = await _db.Courses.FindAsync(id);
            if (course == null) return NotFound();

            // SAFETY CHECK
            if (!course.IsApproved || course.IsRejected)
            {
                TempData["Error"] = "Only approved courses can be published.";
                return RedirectToAction(nameof(Index));
            }

            course.IsPublished = !course.IsPublished;

            _db.AuditLogs.Add(new DB.AuditLog
            {
                Action = course.IsPublished
                    ? $"Published course: {course.Title}"
                    : $"Unpublished course: {course.Title}",
                Timestamp = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ================= TOGGLE PUBLISH (AJAX) =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePublishAjax([FromBody] ToggleVm vm)
        {
            var course = await _db.Courses
                .Include(c => c.Teacher).ThenInclude(t => t.User)
                .FirstOrDefaultAsync(c => c.CourseId == vm.Id);

            if (course == null)
                return Json(new { success = false });

            //  Cannot publish rejected course
            if (course.IsRejected)
            {
                return Json(new
                {
                    success = false,
                    message = "Rejected courses cannot be published."
                });
            }

            course.IsPublished = !course.IsPublished;

            _db.AuditLogs.Add(new DB.AuditLog
            {
                Action = course.IsPublished
                    ? $"Published course: {course.Title}"
                    : $"Unpublished course: {course.Title}",
                Timestamp = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            return Json(new
            {
                success = true,
                isPublished = course.IsPublished
            });
        }

        //Aproval Controller
        // ===================== PENDING COURSES =====================
        public async Task<IActionResult> Approval ()
        {
            var courses = await _db.Courses
                .Include(c => c.Teacher)
                    .ThenInclude(t => t.User)
                .Include(c => c.Category)
                .Where(c => !c.IsApproved && !c.IsRejected)
                .ToListAsync();

            return View(courses);
        }

        // ===================== APPROVE =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var course = await _db.Courses
                .Include(c => c.Teacher)
                    .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null) return NotFound();

            course.IsApproved = true;
            course.IsPublished = true;
            course.IsRejected = false;
            course.RejectionReason = null;

            //AUDIT LOG
            _db.AuditLogs.Add(new DB.AuditLog
            {
                Action = $"Approved course: {course.Title} (Teacher: {course.Teacher!.User!.Email})",
                Timestamp = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ===================== REJECT =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "Rejection reason is required.";
                return RedirectToAction(nameof(Index));
            }

            var course = await _db.Courses
                .Include(c => c.Teacher)
                    .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null) return NotFound();

            course.IsApproved = false;
            course.IsPublished = false;
            course.IsRejected = true;
            course.RejectionReason = reason;

            //AUDIT LOG
            _db.AuditLogs.Add(new DB.AuditLog
            {
                Action = $"Rejected course: {course.Title} | Reason: {reason}",
                Timestamp = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Course Statictics
        public async Task<IActionResult> Statistics()
        {
            var vm = new CourseStatisticsVm
            {
                TotalCourses = await _db.Courses.CountAsync(),

                PendingCourses = await _db.Courses
                    .CountAsync(c => !c.IsApproved && !c.IsRejected),

                ApprovedCourses = await _db.Courses
                    .CountAsync(c => c.IsApproved && !c.IsRejected),

                RejectedCourses = await _db.Courses
                    .CountAsync(c => c.IsRejected),

                PublishedCourses = await _db.Courses
                    .CountAsync(c => c.IsPublished),

                UnpublishedCourses = await _db.Courses
                    .CountAsync(c => !c.IsPublished)
            };

            return View(vm);
        }
    }
}
