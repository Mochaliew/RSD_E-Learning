using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Models;
using RSD_E_Learning.ViewModels;
using static RSD_E_Learning.Models.DB;

namespace RSD_E_Learning.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentCourseController : Controller
    {
        private readonly DB _db;

        public StudentCourseController(DB db)
        {
            _db = db;
        }

        // ================== VIEW PUBLISHED COURSES ==================
        public async Task<IActionResult> Index()
        {
            ViewBag.Categories = await _db.Categories
                .Where(c => !c.IsDeleted)
                .ToListAsync();

            var courses = await _db.Courses
                .Include(c => c.Category)
                .Include(c => c.Teacher)
                    .ThenInclude(t => t.User)
                .Where(c => c.IsApproved && c.IsPublished)
                .ToListAsync();

            return View(courses);
        }


        // ================== COURSE DETAILS ==================
        public async Task<IActionResult> Details(int id)
        {
            var course = await _db.Courses
                .Include(c => c.Category)
                .Include(c => c.Teacher)
                    .ThenInclude(t => t.User)
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
                return NotFound();

            
            bool isEnrolled = false;

            if (User.Identity!.IsAuthenticated && User.IsInRole("Student"))
            {
                var userEmail = User.Identity.Name;

                var student = await _db.Students
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.User!.Email == userEmail);

                if (student != null)
                {
                    isEnrolled = await _db.Enrollments.AnyAsync(e =>
                        e.StudentId == student.StudentId &&
                        e.CourseId == id);
                }
            }

            ViewBag.IsEnrolled = isEnrolled;

            return View(course);
        }



        // ================== ENROLL COURSE ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int courseId)
        {
            var userEmail = User.Identity!.Name;

            var student = await _db.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.User!.Email == userEmail);

            if (student == null)
                return Unauthorized();

            // 1️⃣ Prevent duplicate enrollment
            var alreadyEnrolled = await _db.Enrollments.AnyAsync(e =>
                e.StudentId == student.StudentId &&
                e.CourseId == courseId);

            if (alreadyEnrolled)
            {
                TempData["Message"] = "You are already enrolled in this course.";
                return RedirectToAction("Details", new { id = courseId });
            }

            // 2️⃣ Create enrollment
            var enrollment = new DB.Enrollment
            {
                StudentId = student.StudentId,
                CourseId = courseId,
                PaymentStatus = false
            };

            _db.Enrollments.Add(enrollment);

            // 3️⃣ CREATE PROGRESS ROW (🔥 THIS IS NEW)
            var progress = new DB.StudentCourseProgress
            {
                StudentId = student.StudentId,
                CourseId = courseId,
                ProgressPercentage = 0,
                UpdatedAt = DateTime.UtcNow
            };

            _db.StudentCourseProgresses.Add(progress);

            await _db.SaveChangesAsync();

            TempData["Message"] = "Enrollment successful!";
            return RedirectToAction("MyCourses");
        }



        // ================== MY COURSES ==================
        public async Task<IActionResult> MyCourses()
        {
            var userEmail = User.Identity!.Name;

            var student = await _db.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.User!.Email == userEmail);

            if (student == null)
                return Unauthorized();

            var courses = await _db.Enrollments
                .Where(e => e.StudentId == student.StudentId)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Category)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Teacher)
                        .ThenInclude(t => t.User)
                .Select(e => new StudentMyCourseVm
                {
                    CourseId = e.Course!.CourseId,
                    Title = e.Course.Title,
                    Category = e.Course.Category!.Name,
                    Instructor = e.Course.Teacher!.User!.FullName,

                    ProgressPercentage = _db.StudentCourseProgresses
                        .Where(p => p.StudentId == student.StudentId && p.CourseId == e.Course.CourseId)
                        .Select(p => p.ProgressPercentage)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return View(courses);
        }


        // ==================TO VIEW MATERIAL ==================

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> ViewMaterial(int id)
        {
            var userEmail = User.Identity!.Name;

            var student = await _db.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.User!.Email == userEmail);

            if (student == null)
                return Unauthorized();

            var material = await _db.CourseFiles
                .FirstOrDefaultAsync(f => f.CourseFileId == id && f.IsActive);

            if (material == null)
                return NotFound();

            var existingProgress = await _db.StudentMaterialProgresses
                .FirstOrDefaultAsync(p =>
                    p.StudentId == student.StudentId &&
                    p.CourseFileId == material.CourseFileId);

            if (existingProgress == null)
            {
                var progress = new StudentMaterialProgress
                {
                    StudentId = student.StudentId,
                    CourseFileId = material.CourseFileId,
                    IsCompleted = true,
                    ViewedAt = DateTime.UtcNow
                };

                _db.StudentMaterialProgresses.Add(progress);
                await _db.SaveChangesAsync();

                await UpdateCourseProgress(student.StudentId, material.LessonId);
            }

            return View(material);
        }


        private async Task UpdateCourseProgress(int studentId, int courseId)
        {
            var totalMaterials = await _db.CourseFiles
                .CountAsync(f => f.LessonId == courseId && f.IsActive);

            if (totalMaterials == 0)
                return;

            var completedMaterials = await _db.StudentMaterialProgresses
                .CountAsync(p =>
                    p.StudentId == studentId &&
                    p.IsCompleted &&
                    _db.CourseFiles.Any(f =>
                        f.CourseFileId == p.CourseFileId &&
                        f.LessonId == courseId));

            var percentage = (int)Math.Round(
                (double)completedMaterials / totalMaterials * 100
            );

            var courseProgress = await _db.StudentCourseProgresses
                .FirstOrDefaultAsync(p =>
                    p.StudentId == studentId &&
                    p.CourseId == courseId);

            if (courseProgress == null)
            {
                courseProgress = new StudentCourseProgress
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    ProgressPercentage = percentage,
                    UpdatedAt = DateTime.UtcNow
                };

                _db.StudentCourseProgresses.Add(courseProgress);
            }
            else
            {
                courseProgress.ProgressPercentage = percentage;
                courseProgress.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
        }


        [Authorize(Roles = "Student")]
        public async Task<IActionResult> EnterCourse(int courseId)
        {
            var userEmail = User.Identity!.Name;

            var student = await _db.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.User!.Email == userEmail);

            if (student == null)
                return Unauthorized();

            var isEnrolled = await _db.Enrollments.AnyAsync(e =>
                e.StudentId == student.StudentId &&
                e.CourseId == courseId);

            if (!isEnrolled)
                return Forbid();

            var course = await _db.Courses
                .Include(c => c.Category)
                .Include(c => c.Teacher)
                    .ThenInclude(t => t.User)
                .Include(c => c.CourseFiles)
                .Include(c => c.Assessments) // 🔥 KEEP THIS
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null)
                return NotFound();

            return View(course);
        }




        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Payment(int courseId)
        {
            var userEmail = User.Identity!.Name;

            var student = await _db.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.User!.Email == userEmail);

            if (student == null)
                return Unauthorized();

            //  SAFETY CHECK: Already enrolled → NO PAYMENT
            var alreadyEnrolled = await _db.Enrollments.AnyAsync(e =>
                e.StudentId == student.StudentId &&
                e.CourseId == courseId);

            if (alreadyEnrolled)
            {
                TempData["Message"] = "You are already enrolled in this course.";
                return RedirectToAction("MyCourses");
            }

            var course = await _db.Courses
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null)
                return NotFound();

            return View(course);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> ConfirmPayment(int courseId)
        {
            var userEmail = User.Identity!.Name;

            var student = await _db.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.User!.Email == userEmail);

            if (student == null)
                return Unauthorized();

            var alreadyEnrolled = await _db.Enrollments.AnyAsync(e =>
                e.StudentId == student.StudentId &&
                e.CourseId == courseId);

            if (alreadyEnrolled)
                return RedirectToAction("MyCourses");

            var course = await _db.Courses
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null)
                return NotFound();

            _db.Enrollments.Add(new Enrollment
            {
                StudentId = student.StudentId,
                CourseId = courseId,
                PaymentStatus = true,
                PaymentMethod = "FakeGateway",
                AmountPaid = course.Price
            });

            _db.StudentCourseProgresses.Add(new StudentCourseProgress
            {
                StudentId = student.StudentId,
                CourseId = courseId,
                ProgressPercentage = 0,
                UpdatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            return RedirectToAction("MyCourses");
        }



        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Profile()
        {
            var userEmail = User.Identity!.Name;

            var student = await _db.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.User!.Email == userEmail);

            if (student == null)
                return Unauthorized();

            return View(student);
        }












        //FAKEDATA
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> ByCategory(int categoryId)
        {
            var courses = await _db.Courses
                .Include(c => c.Category)
                .Include(c => c.Teacher)
                    .ThenInclude(t => t.User)
                .Where(c =>
                    c.CategoryId == categoryId &&
                    c.IsApproved &&
                    c.IsPublished)
                .ToListAsync();

            return View(courses);
        }

    }
}
