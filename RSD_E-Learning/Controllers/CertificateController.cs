using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Models;

namespace RSD_E_Learning.Controllers
{
    public class CertificateController : Controller
    {
        private readonly DB _db;
        private readonly IWebHostEnvironment _environment;

        public CertificateController(DB db, IWebHostEnvironment environment)
        {
            _db = db;
            _environment = environment;
        }

        // -------------------- STUDENT: VIEW MY CERTIFICATES --------------------
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyCertificates()
        {
            var studentIdClaim = User.FindFirst("StudentId");
            if (studentIdClaim == null)
                return Unauthorized();

            int studentId = int.Parse(studentIdClaim.Value);

            var certificates = await _db.Certificates
                .Include(c => c.Course)
                    .ThenInclude(c => c!.Category)
                .Include(c => c.Course)
                    .ThenInclude(c => c!.Teacher)
                        .ThenInclude(t => t!.User)
                .Include(c => c.Student)
                    .ThenInclude(s => s!.User)
                .Where(c => c.StudentId == studentId)
                .OrderByDescending(c => c.IssuedDate)
                .ToListAsync();

            return View(certificates);
        }

        // -------------------- VIEW CERTIFICATE (PUBLIC) --------------------
        [AllowAnonymous]
        public async Task<IActionResult> View(int id)
        {
            var certificate = await _db.Certificates
                .Include(c => c.Student)
                    .ThenInclude(s => s!.User)
                .Include(c => c.Course)
                    .ThenInclude(c => c!.Category)
                .Include(c => c.Course)
                    .ThenInclude(c => c!.Teacher)
                        .ThenInclude(t => t!.User)
                .FirstOrDefaultAsync(c => c.CertificateId == id);

            if (certificate == null)
                return NotFound();

            // Generate QR Code URL for verification
            var verificationUrl = Url.Action("Verify", "Certificate",
                new { id = certificate.CertificateId },
                Request.Scheme);

            ViewBag.QRCodeUrl = GenerateQRCodeUrl(verificationUrl ?? "");

            return View(certificate);
        }

        // -------------------- QR VERIFICATION PAGE --------------------
        [AllowAnonymous]
        public async Task<IActionResult> Verify(int id)
        {
            var certificate = await _db.Certificates
                .Include(c => c.Student)
                    .ThenInclude(s => s!.User)
                .Include(c => c.Course)
                    .ThenInclude(c => c!.Category)
                .Include(c => c.Course)
                    .ThenInclude(c => c!.Teacher)
                        .ThenInclude(t => t!.User)
                .FirstOrDefaultAsync(c => c.CertificateId == id);

            if (certificate == null)
            {
                ViewBag.IsValid = false;
                ViewBag.Message = "Certificate not found. This may be a fraudulent certificate.";
                return View((DB.Certificate?)null);
            }

            ViewBag.IsValid = true;
            ViewBag.Message = "This certificate is valid and authentic.";
            return View(certificate);
        }

        // -------------------- GENERATE CERTIFICATE (ADMIN/TEACHER) --------------------
        [Authorize(Roles = "Admin,Teacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(int studentId, int courseId)
        {
            // Check if certificate already exists
            var existingCert = await _db.Certificates
                .FirstOrDefaultAsync(c => c.StudentId == studentId && c.CourseId == courseId);

            if (existingCert != null)
            {
                TempData["ErrorMessage"] = "Certificate already issued for this student.";
                return RedirectToAction("Details", "Courses", new { id = courseId });
            }

            // Verify enrollment and completion
            var enrollment = await _db.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == studentId &&
                                         e.CourseId == courseId &&
                                         e.PaymentStatus);

            if (enrollment == null)
            {
                TempData["ErrorMessage"] = "Student must be enrolled and paid for the course.";
                return RedirectToAction("Details", "Courses", new { id = courseId });
            }

            // Generate unique certificate URL
            string certificateUrl = $"/certificates/{Guid.NewGuid()}.pdf";

            var certificate = new DB.Certificate
            {
                StudentId = studentId,
                CourseId = courseId,
                IssuedDate = DateTime.UtcNow,
                CertificateURL = certificateUrl
            };

            _db.Certificates.Add(certificate);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Certificate generated successfully.";
            return RedirectToAction("View", new { id = certificate.CertificateId });
        }

        // -------------------- DOWNLOAD CERTIFICATE --------------------
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Download(int id)
        {
            var studentIdClaim = User.FindFirst("StudentId");
            if (studentIdClaim == null)
                return Unauthorized();

            int studentId = int.Parse(studentIdClaim.Value);

            var certificate = await _db.Certificates
                .Include(c => c.Course)
                .Include(c => c.Student)
                    .ThenInclude(s => s!.User)
                .FirstOrDefaultAsync(c => c.CertificateId == id && c.StudentId == studentId);

            if (certificate == null)
                return NotFound();

            // Redirect to view for now (you can implement PDF generation later)
            return RedirectToAction("View", new { id = certificate.CertificateId });
        }

        // -------------------- HELPER: GENERATE QR CODE URL (Using Google Charts API) --------------------
        private string GenerateQRCodeUrl(string url)
        {
            // Use Google Charts API as a simple alternative (no external package needed)
            var encodedUrl = Uri.EscapeDataString(url);
            return $"https://chart.googleapis.com/chart?cht=qr&chs=200x200&chl={encodedUrl}";
        }
    }
}