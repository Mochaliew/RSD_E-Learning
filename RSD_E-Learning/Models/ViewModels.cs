using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace RSD_E_Learning.Models
{
    // Teacher View Models //
    public class TeacherCreateVm
    {
        [Required, StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, StringLength(100, MinimumLength = 6), DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required, StringLength(100)]
        [Display(Name = "Subject Area")]
        public string SubjectArea { get; set; } = "";

    }

    public class CreateCourseVm
    {
        [Required, StringLength(150)]
        public string Title { get; set; } = "";

        [Required]
        public int CategoryId { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public IEnumerable<SelectListItem>? CategoryList { get; set; }
    }

    public class TeacherEditVM
    {
        public int TeacherId { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password (Optional)")]
        public string? NewPassword { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Subject Area")]
        public string SubjectArea { get; set; } = "";
    }

    // Student View Models //
    public class StudentRegisterVm
    {
        [Required, StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required, Compare("Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = "";

        [Display(Name = "Class Name")]
        public string? ClassName { get; set; }
    }

    public class StudentEditVM
    {
        public int StudentId { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password (Optional)")]
        public string? NewPassword { get; set; }

        public string? ClassName { get; set; } = "";
    }

    public class StudentListVm
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? ClassName { get; set; }
    }


    // Category View Models //
    public class CategoryCreateVm
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = "";

        [StringLength(255)]
        public string? Description { get; set; }
    }

    public class CategoryEditVm
    {
        public int CategoryId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = "";

        [StringLength(255)]
        public string? Description { get; set; }
    }

    // Course Approval View Models //
    public class CourseApprovalVm
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = "";
        public string TeacherName { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public string Status { get; set; } = "";
    }

    // Course File View Models //
    public class CourseFileVm
    {
        public int FileId { get; set; }
        public int CourseId { get; set; }

        [Required]
        public string FileName { get; set; } = "";

        public string FileType { get; set; } = "";

        public string? Description { get; set; }

        public long Filesize { get; set; }

        public string FileUrl { get; set; } = "";

        public DateTime UploadedAt { get; set; }

        public string UploadedBy { get; set; } = "";
    }

    // Login View Models //
    public class LoginViewModel
    {
        [Required, EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = "";

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = "";

        public bool RememberMe { get; set; }
    }

    // Reset Password View Models //
    public class ResetPasswordVm
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";
    }

    // Admin Dashboard View Models //
    public class AdminDashboardVm
    {
        public int TotalTeachers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalCourses { get; set; }
        public int NewStudentRegistrations { get; set; }
        public List<LatestEnrollmentItem> LatestEnrollments { get; set; } = new List<LatestEnrollmentItem>();
    }

    public class LatestEnrollmentItem
    {
        public string StudentName { get; set; } = "";
        public string CourseTitle { get; set; } = "";
        public DateTime EnrolledAt { get; set; }
    }

    // TransactionList View Models //
    public class TransactionListVm
    {
        public int TransactionId { get; set; }
        public string StudentName { get; set; } = "";
        public string CourseTitle { get; set; } = "";
        public DateTime PaidAt { get; set; }
        public decimal Amount { get; set; }
    }

    // Setting View Models //
    public class SystemSettingsVm
    {
        [Required]
        public string PlatformName { get; set; } = "";
        public string? ContactEmail { get; set; }
        public string? SmtpServer { get; set; }
        public string? SmtpPassword { get; set; }
        public string? FooterText { get; set; }
    }


}
