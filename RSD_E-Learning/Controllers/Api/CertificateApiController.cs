using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Models;
using RSD_E_Learning.Services;

namespace RSD_E_Learning.Controllers.Api
{
    [Route("api/certificate")]
    [ApiController]
    public class CertificateApiController : ControllerBase
    {
        private readonly DB _db;
        private readonly ICertificateService _certificateService;

        public CertificateApiController(DB db, ICertificateService certificateService)
        {
            _db = db;
            _certificateService = certificateService;
        }

        // ==================== /api/certificate/generate ====================
        /// <summary>
        /// Generate a certificate for a student who completed a course
        /// </summary>
        [HttpPost("generate")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Generate([FromBody] GenerateCertificateRequest request)
        {
            try
            {
                // Validate request
                if (request.StudentId <= 0 || request.CourseId <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid student or course ID" });
                }

                // Check if certificate already exists
                var existingCert = await _db.Certificates
                    .FirstOrDefaultAsync(c => c.StudentId == request.StudentId &&
                                             c.CourseId == request.CourseId);

                if (existingCert != null)
                {
                    return Conflict(new
                    {
                        success = false,
                        message = "Certificate already exists for this student",
                        certificateId = existingCert.CertificateId
                    });
                }

                // Verify enrollment
                var enrollment = await _db.Enrollments
                    .Include(e => e.Student)
                        .ThenInclude(s => s!.User)
                    .Include(e => e.Course)
                        .ThenInclude(c => c!.Category)
                    .Include(e => e.Course)
                        .ThenInclude(c => c!.Teacher)
                            .ThenInclude(t => t!.User)
                    .FirstOrDefaultAsync(e => e.StudentId == request.StudentId &&
                                             e.CourseId == request.CourseId);

                if (enrollment == null)
                {
                    return NotFound(new { success = false, message = "Student not enrolled in this course" });
                }

                if (!enrollment.PaymentStatus)
                {
                    return BadRequest(new { success = false, message = "Course payment not completed" });
                }

                // Generate certificate using service
                var certificate = await _certificateService.GenerateCertificateAsync(
                    request.StudentId,
                    request.CourseId
                );

                // Return success with certificate details
                return Ok(new
                {
                    success = true,
                    message = "Certificate generated successfully",
                    certificate = new
                    {
                        certificateId = certificate.CertificateId,
                        certificateNumber = $"CERT-{certificate.CertificateId:D6}",
                        studentName = enrollment.Student?.User?.FullName,
                        courseTitle = enrollment.Course?.Title,
                        issuedDate = certificate.IssuedDate,
                        pdfUrl = certificate.CertificateURL,
                        verificationUrl = Url.Action("Verify", "Certificate",
                            new { id = certificate.CertificateId }, Request.Scheme)
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error generating certificate",
                    error = ex.Message
                });
            }
        }

        // ==================== /api/certificate/verify ====================
        /// <summary>
        /// Verify if a certificate is valid
        /// </summary>
        [HttpGet("verify/{certificateId}")]
        [AllowAnonymous]
        public async Task<IActionResult> Verify(int certificateId)
        {
            try
            {
                var certificate = await _db.Certificates
                    .Include(c => c.Student)
                        .ThenInclude(s => s!.User)
                    .Include(c => c.Course)
                        .ThenInclude(c => c!.Category)
                    .Include(c => c.Course)
                        .ThenInclude(c => c!.Teacher)
                            .ThenInclude(t => t!.User)
                    .FirstOrDefaultAsync(c => c.CertificateId == certificateId);

                if (certificate == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        isValid = false,
                        message = "Certificate not found. This may be a fraudulent certificate."
                    });
                }

                return Ok(new
                {
                    success = true,
                    isValid = true,
                    message = "Certificate is valid and authentic",
                    certificate = new
                    {
                        certificateId = certificate.CertificateId,
                        certificateNumber = $"CERT-{certificate.CertificateId:D6}",
                        student = new
                        {
                            name = certificate.Student?.User?.FullName,
                            email = certificate.Student?.User?.Email
                        },
                        course = new
                        {
                            title = certificate.Course?.Title,
                            category = certificate.Course?.Category?.Name,
                            instructor = certificate.Course?.Teacher?.User?.FullName,
                            subjectArea = certificate.Course?.Teacher?.SubjectArea
                        },
                        issuedDate = certificate.IssuedDate,
                        verifiedAt = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error verifying certificate",
                    error = ex.Message
                });
            }
        }

        // ==================== /api/certificate/history ====================
        /// <summary>
        /// Get certificate history for a student
        /// </summary>
        [HttpGet("history")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> History([FromQuery] int? studentId = null)
        {
            try
            {
                // Get student ID from claims or query parameter
                int currentStudentId;

                if (User.IsInRole("Student"))
                {
                    var studentIdClaim = User.FindFirst("StudentId");
                    if (studentIdClaim == null)
                        return Unauthorized(new { success = false, message = "Student ID not found in claims" });

                    currentStudentId = int.Parse(studentIdClaim.Value);
                }
                else if (studentId.HasValue)
                {
                    currentStudentId = studentId.Value;
                }
                else
                {
                    return BadRequest(new { success = false, message = "Student ID required" });
                }

                var certificates = await _db.Certificates
                    .Include(c => c.Course)
                        .ThenInclude(c => c!.Category)
                    .Include(c => c.Course)
                        .ThenInclude(c => c!.Teacher)
                            .ThenInclude(t => t!.User)
                    .Include(c => c.Student)
                        .ThenInclude(s => s!.User)
                    .Where(c => c.StudentId == currentStudentId)
                    .OrderByDescending(c => c.IssuedDate)
                    .ToListAsync();

                var certificateList = certificates.Select(cert => new
                {
                    certificateId = cert.CertificateId,
                    certificateNumber = $"CERT-{cert.CertificateId:D6}",
                    courseTitle = cert.Course?.Title,
                    courseCategory = cert.Course?.Category?.Name,
                    instructor = cert.Course?.Teacher?.User?.FullName,
                    issuedDate = cert.IssuedDate,
                    pdfUrl = cert.CertificateURL,
                    viewUrl = Url.Action("View", "Certificate",
                        new { id = cert.CertificateId }, Request.Scheme),
                    verificationUrl = Url.Action("Verify", "Certificate",
                        new { id = cert.CertificateId }, Request.Scheme)
                }).ToList();

                return Ok(new
                {
                    success = true,
                    count = certificateList.Count,
                    certificates = certificateList,
                    statistics = new
                    {
                        totalCertificates = certificateList.Count,
                        categoriesCompleted = certificates
                            .Where(c => c.Course?.Category?.Name != null)
                            .GroupBy(c => c.Course!.Category!.Name)
                            .Count(),
                        latestAchievement = certificateList.FirstOrDefault()?.issuedDate
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error retrieving certificate history",
                    error = ex.Message
                });
            }
        }

        // ==================== ADMIN: Get All Certificates ====================
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var query = _db.Certificates
                    .Include(c => c.Student)
                        .ThenInclude(s => s!.User)
                    .Include(c => c.Course)
                        .ThenInclude(c => c!.Category)
                    .Include(c => c.Course)
                        .ThenInclude(c => c!.Teacher)
                            .ThenInclude(t => t!.User)
                    .OrderByDescending(c => c.IssuedDate);

                var total = await query.CountAsync();
                var certificates = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    total = total,
                    page = page,
                    pageSize = pageSize,
                    totalPages = (int)Math.Ceiling(total / (double)pageSize),
                    certificates = certificates.Select(c => new
                    {
                        certificateId = c.CertificateId,
                        certificateNumber = $"CERT-{c.CertificateId:D6}",
                        studentName = c.Student?.User?.FullName,
                        courseTitle = c.Course?.Title,
                        category = c.Course?.Category?.Name,
                        issuedDate = c.IssuedDate
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error retrieving certificates",
                    error = ex.Message
                });
            }
        }
    }

    // ==================== REQUEST MODELS ====================
    public class GenerateCertificateRequest
    {
        public int StudentId { get; set; }
        public int CourseId { get; set; }
    }
}