using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Models;
using System.Security.Claims;
using System.Text;
using static RSD_E_Learning.Models.DB;

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

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("UserId", user.Id.ToString()),
                new Claim("StudentId", teacher.TeacherId.ToString()),
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
            try
            {
                // Load categories for dropdown
                var categories = await _context.Categories
        .OrderBy(c => c.Name)
        .Select(c => new SelectListItem
        {
            Value = c.CategoryId.ToString(),
            Text = c.Name
        })
        .ToListAsync();

                return View(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create course page");
                TempData["ErrorMessage"] = "Error loading page. Please try again.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(
            string courseTitle,
            int category,
            string description,
            List<string> materialType,
            List<string> materialTitle,
            List<IFormFile> materialFile)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(courseTitle) || category == 0 || string.IsNullOrWhiteSpace(description))
            {
                TempData["ErrorMessage"] = "Please fill in all required fields.";
                return RedirectToAction("CreateCourse");
            }

            try
            {
                // Create new course
                var course = new Course
                {
                    Title = courseTitle.Trim(),
                    Description = description.Trim(),
                    CategoryId = category,
                    //TeacherId = teacherId.Value// Set the teacher ID appropriately
                };

                _context.Courses.Add(course);
                await _context.SaveChangesAsync();

                // Process course materials
                if (materialFile != null && materialFile.Count > 0)
                {
                    for (int i = 0; i < materialFile.Count; i++)
                    {
                        var file = materialFile[i];

                        if (file != null && file.Length > 0)
                        {
                            var type = i < materialType.Count ? materialType[i] : "document";
                            var title = i < materialTitle.Count && !string.IsNullOrWhiteSpace(materialTitle[i])
                                ? materialTitle[i]
                                : file.FileName;

                            // Save file and create records
                            /*
                            var filePath = await SaveCourseFileAsync(file, course.CourseId, type);

                            if (!string.IsNullOrEmpty(filePath))
                            {
                                // Create Lesson record
                                var lesson = new Lesson
                                {
                                    CourseId = course.CourseId,
                                    Title = title,
                                    FilePath = filePath,
                                    Content = $"Material Type: {type}"
                                };
                                _context.Lessons.Add(lesson);

                                // Create CourseFile record
                                var courseFile = new CourseFile
                                {
                                    CourseId = course.CourseId,
                                    //TeacherId = teacherId.Value,//
                                    FileName = file.FileName,
                                    FilePath = filePath,
                                    IsActive = true,
                                    UpdateAt = DateTime.UtcNow
                                };
                                _context.CourseFiles.Add(courseFile);
                            }
                            */
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Course created successfully!";
                return RedirectToAction("ViewCourse");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating course");
                TempData["ErrorMessage"] = "An error occurred while creating the course. Please try again.";
                return RedirectToAction("CreateCourse");
            }
        }

        public IActionResult ViewCourse()
        {
            return View();
        }

        public IActionResult CreateAssessment()
        {
            return View();
        }

        public IActionResult ViewAssessment()
        {
            return View();
        }

        public IActionResult TeacherDashboard()
        {
            return View();
        }

    }
}