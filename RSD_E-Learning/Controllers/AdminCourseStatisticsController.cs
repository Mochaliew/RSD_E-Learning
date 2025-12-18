using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Models;

[Authorize(Roles = "Admin")]
public class AdminCourseStatisticsController : Controller
{
    private readonly DB _db;

    public AdminCourseStatisticsController(DB db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
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
