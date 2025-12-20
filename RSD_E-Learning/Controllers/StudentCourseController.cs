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

                    ProgressPercentage =
    (
                        _db.StudentMaterialProgresses.Count(p =>
                            p.StudentId == student.StudentId &&
                            p.IsCompleted &&
                            _db.CourseFiles.Any(f =>
                                f.CourseFileId == p.CourseFileId &&
                                _db.Lessons.Any(l =>
                                    l.LessonId == f.LessonId &&
                                    l.CourseId == e.Course.CourseId
                                )
                            )
                        )
                        * 100
                    )
                    /
                    Math.Max(
                        1,
                        _db.CourseFiles.Count(f =>
                            _db.Lessons.Any(l =>
                                l.LessonId == f.LessonId &&
                                l.CourseId == e.Course.CourseId
                            )
                        )
                    )

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


        private async Task UpdateCourseProgress(int studentId, int lessonId)
        {
            // Get the lesson with its course
            var lesson = await _db.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.LessonId == lessonId);

            if (lesson == null)
                return;

            int courseId = lesson.CourseId;

            // Get ALL lesson IDs in this course
            var lessonIds = await _db.Lessons
                .Where(l => l.CourseId == courseId)
                .Select(l => l.LessonId)
                .ToListAsync();

            // Get TOTAL materials in this course
            var totalMaterials = await _db.CourseFiles
                .CountAsync(f =>
                    lessonIds.Contains(f.LessonId) &&
                    f.IsActive);

            if (totalMaterials == 0)
                return;

            // Get COMPLETED materials by student
            var completedMaterials = await _db.StudentMaterialProgresses
                .CountAsync(p =>
                    p.StudentId == studentId &&
                    p.IsCompleted &&
                    _db.CourseFiles.Any(f =>
                        f.CourseFileId == p.CourseFileId &&
                        lessonIds.Contains(f.LessonId)));

            // Calculate percentage
            int percentage = (int)Math.Round(
                (double)completedMaterials / totalMaterials * 100
            );

            // Update or create course progress
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
            .Include(c => c.Lessons)
                .ThenInclude(l => l.CourseFiles)
            .Include(c => c.Assessments)
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

            var paymentToken = Guid.NewGuid().ToString();  
            ViewBag.PaymentToken = paymentToken;

            return View(course);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> ConfirmPayment(int courseId, string paymentToken)

        {

            if (string.IsNullOrEmpty(paymentToken))
            {
                return BadRequest("Payment verification failed.");
            }

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

            _db.PaymentTransactions.Add(new PaymentTransaction
            {
                StudentId = student.StudentId,
                CourseId = courseId,
                Amount = course.Price,
                PaymentMethod = "FakeGateway",
                TransactionDate = DateTime.UtcNow
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

            var transactions = await _db.PaymentTransactions
                .Where(t => t.StudentId == student.StudentId)
                .Join(
                    _db.Courses,
                    t => t.CourseId,
                    c => c.CourseId,
                    (t, c) => new StudentTransactionVm
                    {
                        CourseTitle = c.Title,
                        Amount = t.Amount,
                        PaymentMethod = t.PaymentMethod,
                        TransactionDate = t.TransactionDate
                    }
                )
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();


            ViewBag.Transactions = transactions;

            return View(student);
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Certificate(int courseId)
        {
            var userEmail = User.Identity!.Name;

            var student = await _db.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.User!.Email == userEmail);

            if (student == null)
                return Unauthorized();

            var progress = await _db.StudentCourseProgresses
                .FirstOrDefaultAsync(p =>
                    p.StudentId == student.StudentId &&
                    p.CourseId == courseId);

            if (progress == null || progress.ProgressPercentage < 100)
                return Forbid(); // ❗ IMPORTANT SECURITY

            var course = await _db.Courses
                .Include(c => c.Teacher)
                    .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null)
                return NotFound();

            ViewBag.StudentName = student.User!.FullName;
            ViewBag.CourseTitle = course.Title;
            ViewBag.Instructor = course.Teacher!.User!.FullName;
            ViewBag.Date = DateTime.UtcNow;

            return View();
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
