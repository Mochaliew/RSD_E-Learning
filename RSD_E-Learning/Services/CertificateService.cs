using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Models;
using System.Text;

namespace RSD_E_Learning.Services
{
    // ==================== INTERFACE ====================
    public interface ICertificateService
    {
        Task<DB.Certificate> GenerateCertificateAsync(int studentId, int courseId);
        Task<byte[]> GeneratePdfAsync(int certificateId);
        Task<string> GenerateQRCodeAsync(string url);
        Task<bool> ValidateCertificateAsync(int certificateId);
        Task<string> GetCertificateNumberAsync(int certificateId);
    }

    // ==================== IMPLEMENTATION ====================
    public class CertificateService : ICertificateService
    {
        private readonly DB _db;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<CertificateService> _logger;

        public CertificateService(
            DB db,
            IWebHostEnvironment environment,
            ILogger<CertificateService> logger)
        {
            _db = db;
            _environment = environment;
            _logger = logger;
        }

        // ==================== GENERATE CERTIFICATE ====================
        public async Task<DB.Certificate> GenerateCertificateAsync(int studentId, int courseId)
        {
            try
            {
                // Verify enrollment exists
                var enrollment = await _db.Enrollments
                    .FirstOrDefaultAsync(e => e.StudentId == studentId &&
                                             e.CourseId == courseId &&
                                             e.PaymentStatus);

                if (enrollment == null)
                {
                    throw new InvalidOperationException("Student must be enrolled and payment completed");
                }

                // Check if certificate already exists
                var existing = await _db.Certificates
                    .FirstOrDefaultAsync(c => c.StudentId == studentId && c.CourseId == courseId);

                if (existing != null)
                {
                    return existing;
                }

                // Create certificate record
                var certificate = new DB.Certificate
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    IssuedDate = DateTime.UtcNow,
                    CertificateURL = "" // Will be updated after PDF generation
                };

                _db.Certificates.Add(certificate);
                await _db.SaveChangesAsync();

                // Generate PDF and update URL
                var pdfBytes = await GeneratePdfAsync(certificate.CertificateId);
                var pdfPath = await SavePdfAsync(certificate.CertificateId, pdfBytes);

                certificate.CertificateURL = pdfPath;
                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    "Certificate generated: ID={CertificateId}, Student={StudentId}, Course={CourseId}",
                    certificate.CertificateId, studentId, courseId);

                return certificate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating certificate for Student {StudentId}, Course {CourseId}",
                    studentId, courseId);
                throw;
            }
        }

        // ==================== GENERATE PDF ====================
        public async Task<byte[]> GeneratePdfAsync(int certificateId)
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
                    throw new InvalidOperationException("Certificate not found");
                }

                // Generate HTML content for PDF
                var htmlContent = GenerateCertificateHtml(certificate);

                // TODO: Use a PDF library like iTextSharp, DinkToPdf, or SelectPdf
                // For now, returning HTML as bytes (you'll need to implement actual PDF generation)
                var bytes = Encoding.UTF8.GetBytes(htmlContent);

                _logger.LogInformation("PDF generated for Certificate {CertificateId}", certificateId);
                return bytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF for Certificate {CertificateId}", certificateId);
                throw;
            }
        }

        // ==================== GENERATE QR CODE ====================
        public async Task<string> GenerateQRCodeAsync(string url)
        {
            try
            {
                // Using Google Charts API for simplicity
                var encodedUrl = Uri.EscapeDataString(url);
                var qrCodeUrl = $"https://chart.googleapis.com/chart?cht=qr&chs=200x200&chl={encodedUrl}";

                _logger.LogInformation("QR Code generated for URL: {Url}", url);
                return await Task.FromResult(qrCodeUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code for URL {Url}", url);
                throw;
            }
        }

        // ==================== VALIDATE CERTIFICATE ====================
        public async Task<bool> ValidateCertificateAsync(int certificateId)
        {
            try
            {
                var exists = await _db.Certificates
                    .AnyAsync(c => c.CertificateId == certificateId);

                _logger.LogInformation(
                    "Certificate validation: ID={CertificateId}, Valid={IsValid}",
                    certificateId, exists);

                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Certificate {CertificateId}", certificateId);
                throw;
            }
        }

        // ==================== GET CERTIFICATE NUMBER ====================
        public async Task<string> GetCertificateNumberAsync(int certificateId)
        {
            try
            {
                var exists = await _db.Certificates
                    .AnyAsync(c => c.CertificateId == certificateId);

                if (!exists)
                {
                    throw new InvalidOperationException("Certificate not found");
                }

                return $"CERT-{certificateId:D6}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting certificate number for ID {CertificateId}", certificateId);
                throw;
            }
        }

        // ==================== PRIVATE HELPERS ====================

        private string GenerateCertificateHtml(DB.Certificate certificate)
        {
            var certificateNumber = $"CERT-{certificate.CertificateId:D6}";
            var studentName = certificate.Student?.User?.FullName ?? "N/A";
            var courseTitle = certificate.Course?.Title ?? "N/A";
            var category = certificate.Course?.Category?.Name ?? "N/A";
            var instructor = certificate.Course?.Teacher?.User?.FullName ?? "N/A";
            var issuedDate = certificate.IssuedDate.ToString("MMMM dd, yyyy");

            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: 'Georgia', serif;
            margin: 0;
            padding: 40px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        }}
        .certificate {{
            background: white;
            padding: 60px;
            max-width: 800px;
            margin: 0 auto;
            border: 10px solid #667eea;
            box-shadow: 0 10px 30px rgba(0,0,0,0.3);
        }}
        .header {{
            text-align: center;
            margin-bottom: 40px;
        }}
        .title {{
            font-size: 48px;
            color: #667eea;
            margin-bottom: 10px;
        }}
        .subtitle {{
            font-size: 20px;
            color: #666;
        }}
        .recipient {{
            text-align: center;
            margin: 40px 0;
        }}
        .name {{
            font-size: 36px;
            font-weight: bold;
            color: #333;
            border-bottom: 3px solid #667eea;
            display: inline-block;
            padding: 10px 40px;
        }}
        .body-text {{
            text-align: center;
            font-size: 18px;
            color: #555;
            line-height: 1.8;
            margin: 30px 0;
        }}
        .course-title {{
            font-size: 28px;
            font-weight: bold;
            color: #667eea;
            margin: 20px 0;
        }}
        .footer {{
            display: flex;
            justify-content: space-between;
            margin-top: 60px;
            padding-top: 20px;
            border-top: 2px solid #ddd;
        }}
        .footer-item {{
            text-align: center;
        }}
        .footer-label {{
            font-size: 12px;
            color: #999;
            text-transform: uppercase;
        }}
        .footer-value {{
            font-size: 14px;
            color: #333;
            font-weight: bold;
        }}
    </style>
</head>
<body>
    <div class='certificate'>
        <div class='header'>
            <div class='title'>🎓 Certificate of Completion</div>
            <div class='subtitle'>This certifies that</div>
        </div>
        
        <div class='recipient'>
            <div class='name'>{studentName}</div>
        </div>
        
        <div class='body-text'>
            has successfully completed the course
            <div class='course-title'>{courseTitle}</div>
            <div>
                <strong>Category:</strong> {category}<br>
                <strong>Instructor:</strong> {instructor}<br>
                <strong>Date Issued:</strong> {issuedDate}
            </div>
        </div>
        
        <div class='footer'>
            <div class='footer-item'>
                <div class='footer-label'>Certificate ID</div>
                <div class='footer-value'>{certificateNumber}</div>
            </div>
            <div class='footer-item'>
                <div class='footer-label'>Verification</div>
                <div class='footer-value'>Scan QR Code</div>
            </div>
        </div>
    </div>
</body>
</html>";
        }

        private async Task<string> SavePdfAsync(int certificateId, byte[] pdfBytes)
        {
            try
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "certificates");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"CERT-{certificateId:D6}.pdf";
                var filePath = Path.Combine(uploadsFolder, fileName);

                await File.WriteAllBytesAsync(filePath, pdfBytes);

                _logger.LogInformation("PDF saved: {FilePath}", filePath);
                return $"/certificates/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving PDF for Certificate {CertificateId}", certificateId);
                throw;
            }
        }
    }
}