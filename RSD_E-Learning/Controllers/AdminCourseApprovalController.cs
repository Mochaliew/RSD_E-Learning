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

    public async Task<IActionResult> Index()
    {
        var courses = await _db.Courses
            .Include(c => c.Teacher)
                .ThenInclude(t => t.User)
            .Include(c => c.Category)
            .Where(c => !c.IsApproved)
            .ToListAsync();

        return View(courses);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course == null) return NotFound();

        course.IsApproved = true;
        course.IsPublished = true;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course == null) return NotFound();

        course.IsApproved = false;
        course.IsPublished = false;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
