using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Models;
using RSD_E_Learning.ViewModels;

namespace RSD_E_Learning.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly DB _db;

        //   constructor injection
        public StudentController(DB db)
        {
            _db = db;
        }

        // ================== DASHBOARD ==================
        public async Task<IActionResult> Dashboard()
        {
            var email = User.Identity!.Name;

            var student = await _db.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.User!.Email == email);

            if (student == null)
                return Unauthorized();

            var enrolledCourseIds = await _db.Enrollments
                .Where(e => e.StudentId == student.StudentId)
                .Select(e => e.CourseId)
                .ToListAsync();

            // ---------------- RECENT LESSONS ----------------
            var recentLessons = await _db.Lessons
                .Where(l => enrolledCourseIds.Contains(l.CourseId))
                .OrderByDescending(l => l.ScheduleDate)
                .Take(3)
                .Select(l => new RecentLessonVm
                {
                    CourseTitle = l.Course!.Title,
                    LessonTitle = l.Title,
                    ScheduleDate = l.ScheduleDate
                })
                .ToListAsync();

            // ---------------- PENDING FINAL EXAMS ----------------
            var pendingFinals = await _db.FinalExams
                .Where(f => enrolledCourseIds.Contains(f.CourseId))
                .Select(f => new
                {
                    Final = f,
                    Attempts = _db.FinalAttempts
                        .Count(a => a.FinalId == f.FinalId && a.StudentId == student.StudentId),
                    Passed = _db.FinalAttempts
                        .Any(a => a.FinalId == f.FinalId && a.StudentId == student.StudentId && a.IsPassed)
                })
                .Where(x => !x.Passed && x.Attempts < 3)
                .OrderBy(x => x.Final.DeadLine)
                .Take(3)
                .Select(x => new PendingFinalExamVm
                {
                    FinalId = x.Final.FinalId,
                    CourseTitle = x.Final.Course!.Title,
                    Title = x.Final.Title,
                    DeadLine = x.Final.DeadLine,
                    AttemptsUsed = x.Attempts
                })
                .ToListAsync();

            var vm = new StudentDashboardVm
            {
                RecentLessons = recentLessons,
                PendingFinalExams = pendingFinals
            };

            return View(vm);
        }


        // ================== PROFILE ==================
        public async Task<IActionResult> Profile()
        {
            var email = User.Identity!.Name;

            var student = await _db.Students
                .Include(s => s.User)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                .FirstOrDefaultAsync(s => s.User!.Email == email);

            if (student == null)
                return NotFound();

            var transactions = await _db.PaymentTransactions
                .Where(t => t.StudentId == student.StudentId)
                .Include(t => t.Course)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            ViewBag.Transactions = transactions;

            return View(student);
        }


        // ================== EDIT PROFILE ==================
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var email = User.Identity!.Name;

            var student = await _db.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.User!.Email == email);

            if (student == null)
                return NotFound();

            var vm = new StudentEditProfileVm
            {
                StudentId = student.StudentId,
                FullName = student.User!.FullName,
                Email = student.User.Email,
                ClassName = student.ClassName
            };

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(StudentEditProfileVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var email = User.Identity!.Name;

            var student = await _db.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.User!.Email == email);

            if (student == null)
                return NotFound();

            student.User!.FullName = model.FullName;
            student.ClassName = model.ClassName;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Profile updated successfully";

            return RedirectToAction(nameof(EditProfile));
        }


    }
}
