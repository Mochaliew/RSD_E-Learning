using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Services;
using RSD_E_Learning.Models;
using System.Text;

namespace RSD_E_Learning.Services
{
    public class CertificateService : ICertificateService
    {
        private readonly DB _db;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<CertificateService> _logger;

        public CertificateService(DB db, IWebHostEnvironment env, ILogger<CertificateService> logger)
        {
            _db = db;
            _env = env;
            _logger = logger;
        }

        // ================= GENERATE CERTIFICATE =================
        public async Task<DB.Certificate> GenerateCertificateAsync(int studentId, int courseId)
        {
            var enrollment = await _db.Enrollments
                .FirstOrDefaultAsync(e =>
                    e.StudentId == studentId &&
                    e.CourseId == courseId &&
                    e.PaymentStatus);

            if (enrollment == null)
                throw new InvalidOperationException("Student must be enrolled and paid.");

            var existing = await _db.Certificates
                .FirstOrDefaultAsync(c => c.StudentId == studentId && c.CourseId == courseId);

            if (existing != null)
                return existing;

            var cert = new DB.Certificate
            {
                StudentId = studentId,
                CourseId = courseId,
                IssuedDate = DateTime.UtcNow
            };

            _db.Certificates.Add(cert);
            await _db.SaveChangesAsync();

            var pdfBytes = await GeneratePdfAsync(cert.CertificateId);
            cert.CertificateURL = await SavePdfAsync(cert.CertificateId, pdfBytes);

            await _db.SaveChangesAsync();
            return cert;
        }

        // ================= GENERATE PDF =================
        public async Task<byte[]> GeneratePdfAsync(int certificateId)
        {
            var cert = await _db.Certificates
                .Include(c => c.Student).ThenInclude(s => s!.User)
                .Include(c => c.Course).ThenInclude(c => c!.Teacher).ThenInclude(t => t!.User)
                .Include(c => c.Course).ThenInclude(c => c!.Category)
                .FirstAsync(c => c.CertificateId == certificateId);

            // 1️⃣ LOAD TEMPLATE FROM SYSTEM SETTINGS
            var settings = await _db.SystemSettings.FirstAsync();

            if (string.IsNullOrEmpty(settings.CertificateTemplatePath))
                throw new InvalidOperationException("Certificate template not configured.");

            var templatePath = Path.Combine(
                _env.WebRootPath,
                settings.CertificateTemplatePath.Replace("/", Path.DirectorySeparatorChar.ToString())
            );

            if (!File.Exists(templatePath))
                throw new FileNotFoundException("Certificate template not found", templatePath);

            var html = await File.ReadAllTextAsync(templatePath);

            // 2️⃣ REPLACE PLACEHOLDERS
            html = html
                .Replace("{{StudentName}}", cert.Student!.User!.FullName)
                .Replace("{{CourseTitle}}", cert.Course!.Title)
                .Replace("{{Instructor}}", cert.Course.Teacher!.User!.FullName)
                .Replace("{{IssuedDate}}", cert.IssuedDate.ToString("dd MMM yyyy"))
                .Replace("{{CertificateNumber}}", $"CERT-{certificateId:D6}");

            // 3️⃣ CONVERT HTML → PDF (TEMP)
            // Replace with DinkToPdf / QuestPDF later
            return Encoding.UTF8.GetBytes(html);
        }

        // ================= SAVE PDF =================
        private async Task<string> SavePdfAsync(int certificateId, byte[] pdfBytes)
        {
            var folder = Path.Combine(_env.WebRootPath, "certificates");
            Directory.CreateDirectory(folder);

            var fileName = $"CERT-{certificateId:D6}.pdf";
            var fullPath = Path.Combine(folder, fileName);

            await File.WriteAllBytesAsync(fullPath, pdfBytes);
            return $"/certificates/{fileName}";
        }

        // ================= OTHER METHODS (UNCHANGED) =================
        public Task<string> GenerateQRCodeAsync(string url)
            => Task.FromResult($"https://chart.googleapis.com/chart?cht=qr&chs=200x200&chl={Uri.EscapeDataString(url)}");

        public Task<bool> ValidateCertificateAsync(int certificateId)
            => _db.Certificates.AnyAsync(c => c.CertificateId == certificateId);

        public Task<string> GetCertificateNumberAsync(int certificateId)
            => Task.FromResult($"CERT-{certificateId:D6}");
    }
}
