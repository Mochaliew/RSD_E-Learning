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

        // -------------------- LOGIN --------------------
        [HttpGet]
        public IActionResult TeacherLogin()
        {
            return View();
        }

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



        // -------------------- PASSWORD HASHING --------------------
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

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }


        public IActionResult TeacherIndex()
        {
            return View();
        }

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

            // Get current logged-in user id from claims
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null)
            {
                TempData["ErrorMessage"] = "User not found. Please login again.";
                return RedirectToAction("TeacherLogin");
            }

            int userId = int.Parse(userIdClaim.Value);

            // Get teacher profile
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
                TeacherId = teacher.TeacherId,

                IsApproved = false,
                IsPublished = false
            };


            _context.Courses.Add(course);
            await _context.SaveChangesAsync();   // ✅ COURSE SAVED SUCCESSFULLY

            TempData["SuccessMessage"] =
                "Course submitted successfully and pending admin approval.";

            return RedirectToAction("CreateCourse");
        }

        [HttpGet]
        public async Task<IActionResult> UploadMaterial(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);

            if (course == null || !course.IsApproved)
            {
                return Unauthorized();
            }

            var vm = new UploadCourseFileVm
            {
                CourseId = courseId
            };

            ViewBag.CourseTitle = course.Title;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadMaterial(UploadCourseFileVm model)
        {
            if (!ModelState.IsValid)
            {
                var course = await _context.Courses.FindAsync(model.CourseId);
                if (course != null)
                {
                    ViewBag.CourseTitle = course.Title;
                }
                return View(model);
            }

            var teacherIdClaim = User.FindFirst("TeacherId");
            if (teacherIdClaim == null)
                return Unauthorized();

            int teacherId = int.Parse(teacherIdClaim.Value);

            // Ensure course belongs to this teacher
            var courseCheck = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == model.CourseId &&
                                          c.TeacherId == teacherId &&
                                          c.IsApproved);

            if (courseCheck == null)
                return Unauthorized();

            // FILE SAVE
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "coursefiles");
            Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid() + Path.GetExtension(model.materialFile.FileName);
            string physicalPath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await model.materialFile.CopyToAsync(stream);
            }

            // DATABASE SAVE
            var courseFile = new DB.CourseFile
            {
                CourseId = model.CourseId,
                FileName = model.materialTitle,
                FilePath = "/coursefiles/" + uniqueFileName,
                FileType = model.materialType, // Use the selected type
                IsActive = true,
                UpdateAt = DateTime.UtcNow
            };

            _context.CourseFiles.Add(courseFile);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Material uploaded successfully.";
            return RedirectToAction("ViewCourse", "Teacher"); // Fixed: removed the id parameter
        }


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
                // Load all courses for this teacher with related data
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


        // GET: View Course Detail
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

            var courseFiles = await _context.CourseFiles
                .Where(f => f.CourseId == id && f.IsActive)
                .OrderByDescending(f => f.UpdateAt)
                .ToListAsync();

            var assessments = await _context.Assessments
                .Where(a => a.CourseId == id)
                .OrderByDescending(a => a.AssessmentId)
                .ToListAsync();

            // Get question counts for each assessment
            foreach (var assessment in assessments)
            {
                var questionCount = await _context.AssessmentQuestions
                    .CountAsync(q => q.AssessmentId == assessment.AssessmentId);

                // Store count in ViewBag or add a property if needed
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
                CourseFiles = courseFiles,
                Assessments = assessments,
                Categories = categories
            };

            return View(viewModel);
        }

        // POST: Update Course Info
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

        public IActionResult CreateAssessment()
        {
            return View();
        }


        public IActionResult TeacherDashboard()
        {
            return View();
        }

        [HttpPost]
        [Route("api/teacher/create-assessment")]
        public async Task<IActionResult> CreateAssessmentApi(
    [FromBody] CreateAssessmentVm model)
        {
            var teacherIdClaim = User.FindFirst("TeacherId");
            if (teacherIdClaim == null)
                return Unauthorized();

            int teacherId = int.Parse(teacherIdClaim.Value);

            var course = await _context.Courses
                .FirstOrDefaultAsync(c =>
                    c.CourseId == model.CourseId &&
                    c.TeacherId == teacherId &&
                    c.IsApproved);

            if (course == null)
                return BadRequest("Invalid course.");

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

            return Ok(new { message = "Assessment created successfully" });
        }
    }
}