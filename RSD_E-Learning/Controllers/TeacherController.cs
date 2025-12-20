using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RSD_E_Learning.Models;
using System.Security.Claims;
using System.Text;


namespace RSD_E_Learning.Controllers
{
    public class TeacherController : Controller
    {
        // ------------------------- SETUP DB ------------------------- // 

        private readonly DB _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<TeacherController> _logger;

        public TeacherController(
            DB context,
            IWebHostEnvironment environment,
            ILogger<TeacherController> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        // ------------------------- LOGIN [GET] ------------------------- // 

        [HttpGet]
        public IActionResult TeacherLogin()
        {
            return View();
        }

        // ------------------------- LOGIN [POST] ------------------------- // 

        [HttpPost]
        public async Task<IActionResult> TeacherLogin(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.Role == DB.UserRole.Teacher);

            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View();
            }

            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (teacher == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View();
            }

            if (!teacher.IsActive)
            {
                ModelState.AddModelError(
                    "",
                    "Your account has been deactivated. Please contact administrator."
                );
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("UserId", user.Id.ToString()),
                new Claim("TeacherId", teacher.TeacherId.ToString()),
                new Claim("Role", "Teacher")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity)
            );

            return RedirectToAction("TeacherIndex", "Teacher");
        }

        // -------------------- PASSWORD HASHING -------------------- // 

        private string HashPassword(string password)
        {
            byte[] salt = Encoding.UTF8.GetBytes("STATIC-SALT-CHANGE-LATER");

            return Convert.ToBase64String(
                KeyDerivation.Pbkdf2(
                    password,
                    salt,
                    KeyDerivationPrf.HMACSHA256,
                    10000,
                    32
                )
            );
        }

        // ------------------------- VERIFY PASSWORD ------------------------- // 

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        // ------------------------- TEACHERINDEX ------------------------- // 

        public IActionResult TeacherIndex()
        {
            return View();
        }

        // ------------------------- CREATECOURSE [GET] ------------------------- // 

        [HttpGet]
        public async Task<IActionResult> CreateCourse()
        {
            var model = new CreateCourseVm
            {
                CategoryList = await _context.Categories
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem
                    {
                        Value = c.CategoryId.ToString(),
                        Text = c.Name
                    })
                    .ToListAsync()
            };

            return View(model);
        }

        // ------------------------- CREATECOURSE [POST] ------------------------- // 

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(CreateCourseVm model)
        {
            if (!ModelState.IsValid)
            {
                model.CategoryList = await _context.Categories
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem
                    {
                        Value = c.CategoryId.ToString(),
                        Text = c.Name
                    })
                    .ToListAsync();

                return View(model);
            }

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null)
            {
                TempData["ErrorMessage"] = "User not found. Please login again.";
                return RedirectToAction("TeacherLogin");
            }

            int userId = int.Parse(userIdClaim.Value);
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
            if (teacher == null)
            {
                TempData["ErrorMessage"] = "Teacher profile not found.";
                return RedirectToAction("TeacherLogin");
            }

            var course = new DB.Course
            {
                Title = model.Title,
                CategoryId = model.CategoryId,
                Description = model.Description,
                Price = model.Price,
                TeacherId = teacher.TeacherId,

                IsApproved = false,
                IsPublished = false
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();   

            TempData["SuccessMessage"] =
                "Course submitted successfully and pending admin approval.";

            return RedirectToAction("CreateCourse");
        }

        // ------------------------- VIEWCOURSE [GET] ------------------------- // 

        [HttpGet]
        public async Task<IActionResult> ViewCourse()
        {
            // Get current teacher ID from claims
            var teacherIdClaim = User.FindFirst("TeacherId");
            if (teacherIdClaim == null)
            {
                TempData["ErrorMessage"] = "Please login again.";
                return RedirectToAction("TeacherLogin");
            }

            int teacherId = int.Parse(teacherIdClaim.Value);

            try
            {
                var courses = await _context.Courses
                    .Include(c => c.Category)
                    .Include(c => c.Enrollments)
                    .Include(c => c.Lessons)
                    .Include(c => c.Assessments)
                    .Where(c => c.TeacherId == teacherId)
                    .OrderByDescending(c => c.CourseId)
                    .ToListAsync();

                return View(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading courses for teacher {TeacherId}", teacherId);
                TempData["ErrorMessage"] = "Error loading courses. Please try again.";
                return View(new List<DB.Course>());
            }
        }

        // ------------------------- COURSEDETAIL [GET] ------------------------- //

        [HttpGet]
        public async Task<IActionResult> CourseDetail(int id)
        {
            var teacherIdClaim = User.FindFirst("TeacherId");
            if (teacherIdClaim == null)
            {
                return RedirectToAction("TeacherLogin");
            }

            int teacherId = int.Parse(teacherIdClaim.Value);

            var course = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.CourseId == id && c.TeacherId == teacherId);

            if (course == null)
            {
                TempData["ErrorMessage"] = "Course not found or you don't have permission to view it.";
                return RedirectToAction("ViewCourse");
            }

            // Get lessons with their files
            var lessons = await _context.Lessons
                .Where(l => l.CourseId == id)
                .OrderBy(l => l.ScheduleDate)
                .ThenBy(l => l.Title)
                .ToListAsync();

            var lessonsWithFiles = new List<LessonWithFilesVm>();
            foreach (var lesson in lessons)
            {
                var files = await _context.CourseFiles
                    .Where(f => f.LessonId == lesson.LessonId)
                    .ToListAsync();

                lessonsWithFiles.Add(new LessonWithFilesVm
                {
                    Lesson = lesson,
                    Files = files
                });
            }

            var assessments = await _context.Assessments
                .Where(a => a.CourseId == id)
                .OrderByDescending(a => a.AssessmentId)
                .ToListAsync();

            // Get question counts for each assessment
            foreach (var assessment in assessments)
            {
                var questionCount = await _context.AssessmentQuestions
                    .CountAsync(q => q.AssessmentId == assessment.AssessmentId);

                ViewBag.QuestionCounts = ViewBag.QuestionCounts ?? new Dictionary<int, int>();
                ((Dictionary<int, int>)ViewBag.QuestionCounts)[assessment.AssessmentId] = questionCount;
            }

            var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.Name
                })
                .ToListAsync();

            var viewModel = new CourseDetailVm
            {
                Course = course,
                LessonsWithFiles = lessonsWithFiles,
                Assessments = assessments,
                Categories = categories
            };

            return View(viewModel);
        }

        // ------------------------- UPDATECOURSE [POST] ------------------------- // 

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCourseInfo(int CourseId, string Title, int CategoryId, string Description)
        {
            var teacherIdClaim = User.FindFirst("TeacherId");
            if (teacherIdClaim == null)
            {
                return Unauthorized();
            }

            int teacherId = int.Parse(teacherIdClaim.Value);

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == CourseId && c.TeacherId == teacherId);

            if (course == null)
            {
                TempData["ErrorMessage"] = "Course not found.";
                return RedirectToAction("ViewCourse");
            }

            course.Title = Title;
            course.CategoryId = CategoryId;
            course.Description = Description;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Course information updated successfully.";
            return RedirectToAction("CourseDetail", new { id = CourseId });
        }

        // ------------------------- VIEWLESSON [GET] ------------------------- // 

        [HttpGet]
        public async Task<IActionResult> ViewLesson()
        {
            var teacherIdClaim = User.FindFirst("TeacherId");
            if (teacherIdClaim == null)
            {
                return RedirectToAction("TeacherLogin");
            }

            int teacherId = int.Parse(teacherIdClaim.Value);

            // Get all courses for this teacher
            var courses = await _context.Courses
                .Include(c => c.Category)
                .Where(c => c.TeacherId == teacherId && c.IsApproved)
                .OrderBy(c => c.Title)
                .ToListAsync();

            var courseLessonList = new List<CourseLessonVm>();

            foreach (var course in courses)
            {
                var lessons = await _context.Lessons
                    .Where(l => l.CourseId == course.CourseId)
                    .OrderBy(l => l.ScheduleDate)
                    .ThenBy(l => l.Title)
                    .ToListAsync();

                var lessonsWithFiles = new List<LessonWithFilesVm>();
                foreach (var lesson in lessons)
                {
                    var files = await _context.CourseFiles
                        .Where(f => f.LessonId == lesson.LessonId)
                        .ToListAsync();

                    lessonsWithFiles.Add(new LessonWithFilesVm
                    {
                        Lesson = lesson,
                        Files = files
                    });
                }

                courseLessonList.Add(new CourseLessonVm
                {
                    Course = course,
                    LessonsWithFiles = lessonsWithFiles
                });
            }

            return View(courseLessonList);
        }

        // ------------------------- CREATELESSON [GET] ------------------------- // 

        [HttpGet]
        public async Task<IActionResult> CreateLesson(int courseId)
        {
            var teacherIdClaim = User.FindFirst("TeacherId");
            if (teacherIdClaim == null)
            {
                return RedirectToAction("TeacherLogin");
            }

            int teacherId = int.Parse(teacherIdClaim.Value);

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.TeacherId == teacherId);

            if (course == null)
            {
                TempData["ErrorMessage"] = "Course not found or unauthorized.";
                return RedirectToAction("ViewLesson");
            }

            ViewBag.CourseName = course.Title;

            var model = new CreateLessonVm
            {
                CourseId = courseId
            };

            return View(model);
        }

        // ------------------------- CREATELESSON [POST] ------------------------- // 

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLesson(CreateLessonVm model, string LessonDate, string LessonTime, string LessonType, IFormFile MaterialFile)
        {
            // Remove MaterialFile from ModelState since it's not part of the view model
            ModelState.Remove(nameof(MaterialFile));

            // Validate based on lesson type
            if (LessonType == "online")
            {
                // For online meetings, MeetLink is required
                if (string.IsNullOrWhiteSpace(model.MeetLink))
                {
                    ModelState.AddModelError(nameof(model.MeetLink), "Meeting link is required for online lessons.");
                }
            }
            else if (LessonType == "pdf" || LessonType == "video")
            {
                // For file-based lessons, MaterialFile is required
                if (MaterialFile == null || MaterialFile.Length == 0)
                {
                    ModelState.AddModelError("MaterialFile", "A file is required for this lesson type.");
                }
            }
            else if (string.IsNullOrEmpty(LessonType))
            {
                ModelState.AddModelError("LessonType", "Please select a lesson type.");
            }

            if (!ModelState.IsValid)
            {
                var course = await _context.Courses.FindAsync(model.CourseId);
                if (course != null)
                {
                    ViewBag.CourseName = course.Title;
                }
                return View(model);
            }

            var teacherIdClaim = User.FindFirst("TeacherId");
            if (teacherIdClaim == null)
            {
                return Unauthorized();
            }

            int teacherId = int.Parse(teacherIdClaim.Value);

            // Verify course belongs to teacher
            var courseCheck = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == model.CourseId && c.TeacherId == teacherId);

            if (courseCheck == null)
            {
                TempData["ErrorMessage"] = "Course not found or unauthorized.";
                return RedirectToAction("ViewLesson");
            }

            // Combine date and time
            DateTime? scheduledDateTime = null;
            if (!string.IsNullOrEmpty(LessonDate) && !string.IsNullOrEmpty(LessonTime))
            {
                try
                {
                    var datePart = DateTime.Parse(LessonDate);
                    var timePart = TimeSpan.Parse(LessonTime);
                    scheduledDateTime = datePart.Add(timePart);
                }
                catch
                {
                    TempData["ErrorMessage"] = "Invalid date or time format.";
                    var course = await _context.Courses.FindAsync(model.CourseId);
                    if (course != null)
                    {
                        ViewBag.CourseName = course.Title;
                    }
                    return View(model);
                }
            }

            // Create lesson
            var lesson = new DB.Lesson
            {
                CourseId = model.CourseId,
                Title = model.Title,
                MeetLink = LessonType == "online" ? (model.MeetLink ?? "") : "",
                Description = model.Description ?? "",
                ScheduleDate = scheduledDateTime
            };

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            // Handle file upload if provided (for PDF or Video)
            if (MaterialFile != null && MaterialFile.Length > 0 && (LessonType == "pdf" || LessonType == "video"))
            {
                // Validate file size
                long maxFileSize = LessonType == "pdf" ? 50 * 1024 * 1024 : 500 * 1024 * 1024; // 50MB for PDF, 500MB for video

                if (MaterialFile.Length > maxFileSize)
                {
                    TempData["ErrorMessage"] = $"File size exceeds the maximum limit of {(maxFileSize / 1024 / 1024)}MB.";
                    return RedirectToAction("CreateLesson", new { courseId = model.CourseId });
                }

                // Validate file extension
                string[] allowedExtensions = LessonType == "pdf"
                    ? new[] { ".pdf" }
                    : new[] { ".mp4", ".avi", ".mov", ".mkv" };

                string fileExtension = Path.GetExtension(MaterialFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    TempData["ErrorMessage"] = $"Invalid file type. Allowed types: {string.Join(", ", allowedExtensions)}";
                    return RedirectToAction("CreateLesson", new { courseId = model.CourseId });
                }

                string uploadsFolder = Path.Combine(_environment.WebRootPath, "coursefiles");
                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid() + fileExtension;
                string physicalPath = Path.Combine(uploadsFolder, uniqueFileName);

                try
                {
                    using (var stream = new FileStream(physicalPath, FileMode.Create))
                    {
                        await MaterialFile.CopyToAsync(stream);
                    }

                    // Save to CourseFile table - linked to Lesson
                    var courseFile = new DB.CourseFile
                    {
                        LessonId = lesson.LessonId,
                        FilePath = "/coursefiles/" + uniqueFileName,
                        FileType = LessonType,
                        UpdateAt = DateTime.UtcNow
                    };

                    _context.CourseFiles.Add(courseFile);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // Log the error
                    TempData["ErrorMessage"] = "Error uploading file. Please try again.";
                    return RedirectToAction("CreateLesson", new { courseId = model.CourseId });
                }
            }

            TempData["SuccessMessage"] = "Lesson created successfully.";
            return RedirectToAction("CourseDetail", new { id = model.CourseId });
        }

        // ------------------------- EDITLESSON [GET] ------------------------- // 

        [HttpGet]
        public async Task<IActionResult> EditLesson(int id)
        {
            var teacherIdClaim = User.FindFirst("TeacherId");
            if (teacherIdClaim == null)
            {
                return RedirectToAction("TeacherLogin");
            }

            int teacherId = int.Parse(teacherIdClaim.Value);

            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.LessonId == id);

            if (lesson == null || lesson.Course.TeacherId != teacherId)
            {
                TempData["ErrorMessage"] = "Lesson not found or unauthorized.";
                return RedirectToAction("ViewLesson");
            }

            // Get existing files for this lesson
            var existingFiles = await _context.CourseFiles
                .Where(f => f.LessonId == lesson.LessonId)
                .ToListAsync();

            ViewBag.CourseName = lesson.Course.Title;

            var model = new EditLessonVm
            {
                LessonId = lesson.LessonId,
                CourseId = lesson.CourseId,
                Title = lesson.Title,
                MeetLink = lesson.MeetLink,
                Description = lesson.Description,
                ScheduleDate = lesson.ScheduleDate,
                ExistingFiles = existingFiles
            };

            return View(model);
        }

        // ------------------------- EDITLESSON [POST] ------------------------- // 

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLesson(EditLessonVm model, string LessonDate, string LessonTime, string LessonType, IFormFile MaterialFile)
        {
            if (!ModelState.IsValid)
            {
                var course = await _context.Courses.FindAsync(model.CourseId);
                if (course != null)
                {
                    ViewBag.CourseName = course.Title;
                }
                return View(model);
            }

            var teacherIdClaim = User.FindFirst("TeacherId");
            if (teacherIdClaim == null)
            {
                return Unauthorized();
            }

            int teacherId = int.Parse(teacherIdClaim.Value);

            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.LessonId == model.LessonId);

            if (lesson == null || lesson.Course.TeacherId != teacherId)
            {
                TempData["ErrorMessage"] = "Lesson not found or unauthorized.";
                return RedirectToAction("ViewLesson");
            }

            // Combine date and time
            DateTime? scheduledDateTime = null;
            if (!string.IsNullOrEmpty(LessonDate) && !string.IsNullOrEmpty(LessonTime))
            {
                var datePart = DateTime.Parse(LessonDate);
                var timePart = TimeSpan.Parse(LessonTime);
                scheduledDateTime = datePart.Add(timePart);
            }

            lesson.Title = model.Title;
            lesson.MeetLink = model.MeetLink ?? "";
            lesson.Description = model.Description ?? "";
            lesson.ScheduleDate = scheduledDateTime;

            await _context.SaveChangesAsync();

            // Handle new file upload if provided
            if (MaterialFile != null && MaterialFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "coursefiles");
                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid() + Path.GetExtension(MaterialFile.FileName);
                string physicalPath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(physicalPath, FileMode.Create))
                {
                    await MaterialFile.CopyToAsync(stream);
                }

                // Save to CourseFile table - linked to Lesson
                var courseFile = new DB.CourseFile
                {
                    LessonId = lesson.LessonId,
                    FilePath = "/coursefiles/" + uniqueFileName,
                    FileType = LessonType ?? "file",
                    UpdateAt = DateTime.UtcNow
                };

                _context.CourseFiles.Add(courseFile);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Lesson updated successfully.";
            return RedirectToAction("CourseDetail", new { id = model.CourseId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLessonFile(int fileId)
        {
            var teacherIdClaim = User.FindFirst("TeacherId");
            if (teacherIdClaim == null)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            int teacherId = int.Parse(teacherIdClaim.Value);

            var file = await _context.CourseFiles
                .Include(f => f.Lesson)
                .ThenInclude(l => l.Course)
                .FirstOrDefaultAsync(f => f.CourseFileId == fileId);

            if (file == null || file.Lesson.Course.TeacherId != teacherId)
            {
                return Json(new { success = false, message = "File not found or unauthorized" });
            }

            // Delete physical file
            try
            {
                var physicalPath = Path.Combine(_environment.WebRootPath, file.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting physical file");
            }

            // Remove from database
            _context.CourseFiles.Remove(file);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "File deleted successfully" });
        }

        // ------------------------- CREATEASSESSMENT [POST] ------------------------- // 

        [HttpPost]
        [Route("api/teacher/create-assessment")]
        public async Task<IActionResult> CreateAssessmentApi(
        [FromBody] CreateAssessmentVm model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var teacherIdClaim = User.FindFirst("TeacherId");
            if (teacherIdClaim == null)
                return Unauthorized();

            int teacherId = int.Parse(teacherIdClaim.Value);

            if (model.CourseId <= 0)
                return BadRequest("CourseId is required.");

            var course = await _context.Courses
                .FirstOrDefaultAsync(c =>
                    c.CourseId == model.CourseId &&
                    c.TeacherId == teacherId);

            if (course == null)
                return BadRequest("Invalid or unauthorized course.");

            var assessment = new DB.Assessment
            {
                CourseId = model.CourseId,
                Title = model.Title,
                TotalMarks = model.Questions.Count,
                DeadLine = model.DeadLine
            };

            _context.Assessments.Add(assessment);
            await _context.SaveChangesAsync();

            foreach (var q in model.Questions)
            {
                _context.AssessmentQuestions.Add(new DB.AssessmentQuestion
                {
                    AssessmentId = assessment.AssessmentId,
                    QuestionDetail = q.QuestionDetail,
                    AnswerA = q.AnswerA,
                    AnswerB = q.AnswerB,
                    AnswerC = q.AnswerC,
                    AnswerD = q.AnswerD,
                    CorrectAnswer = q.CorrectAnswer
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Assessment and questions saved successfully",
                assessmentId = assessment.AssessmentId
            });
        }
        [HttpGet]
        public IActionResult CreateAssessment(int courseId)
        {
            ViewBag.CourseId = courseId;
            return View();
        }
    }
}