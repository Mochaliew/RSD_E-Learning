using RSD_E_Learning.Models;

namespace RSD_E_Learning.Services
{
    public interface ICertificateService
    {
        Task<DB.Certificate> GenerateCertificateAsync(int studentId, int courseId);
        Task<byte[]> GeneratePdfAsync(int certificateId);
        Task<string> GenerateQRCodeAsync(string url);
        Task<bool> ValidateCertificateAsync(int certificateId);
        Task<string> GetCertificateNumberAsync(int certificateId);
    }
}
