using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Models;

[Authorize(Roles = "Admin")]
public class AdminCourseApprovalController : Controller
{
    private readonly DB _db;

    public AdminCourseApprovalController(DB db)
    {
        _db = db;
    }

    // ===================== PENDING COURSES =====================
    public async Task<IActionResult> Index()
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
}
