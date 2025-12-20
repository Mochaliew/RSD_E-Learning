using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace RSD_E_Learning.Models
{
    // -------------------------------- Teacher View Models --------------------------------------------- //

    // Teacher Create View Models //
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

    // Course View Models //
    public class CreateCourseVm
    {
        [Required, StringLength(150)]
        public string Title { get; set; } = "";

        [Required]
        public int CategoryId { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        public IEnumerable<SelectListItem>? CategoryList { get; set; }
    }

    // Course Detail View Models //
    public class CourseDetailVm
    {
        public DB.Course Course { get; set; } = new();
        public List<LessonWithFilesVm> LessonsWithFiles { get; set; } = new();
        public List<DB.Assessment> Assessments { get; set; } = new();
        public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
    }

    public class LessonWithFilesVm
    {
        public DB.Lesson Lesson { get; set; } = new();
        public List<DB.CourseFile> Files { get; set; } = new();
    }


    // Lesson View Models //
    public class CourseLessonVm
    {
        public DB.Course Course { get; set; } = new();
        public List<LessonWithFilesVm> LessonsWithFiles { get; set; } = new();
    }

    // Lesson Create View Models //
    public class CreateLessonVm
    {
        public int CourseId { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Lesson Title")]
        public string Title { get; set; } = "";

        [StringLength(200)]
        [Display(Name = "Meeting Link")]
        public string? MeetLink { get; set; }

        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Scheduled Date & Time")]
        public DateTime? ScheduledDate { get; set; }
    }

    // Lesson Edit View Models //
    public class EditLessonVm
    {
        public int LessonId { get; set; }
        public int CourseId { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Lesson Title")]
        public string Title { get; set; } = "";

        [StringLength(200)]
        [Display(Name = "Meeting Link")]
        public string? MeetLink { get; set; }

        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Scheduled Date & Time")]
        public DateTime? ScheduledDate { get; set; }

        public List<DB.CourseFile> ExistingFiles { get; set; } = new();


        // Teacher Edit View Models //
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

        // Teacher Reset Password View Models //
        public class ResetTeacherPasswordVm
        {
            [Required]
            public int UserId { get; set; }

            [Required]
            [StringLength(100, MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string NewPassword { get; set; } = "";

            [Required]
            [DataType(DataType.Password)]
            [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
            public string ConfirmPassword { get; set; } = "";
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

        // Course Statistics View Models //
        public class CourseStatisticsVm
        {
            public int TotalCourses { get; set; }
            public int PendingCourses { get; set; }
            public int ApprovedCourses { get; set; }
            public int RejectedCourses { get; set; }
            public int PublishedCourses { get; set; }
            public int UnpublishedCourses { get; set; }
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
            public int PendingCourses { get; set; }
            public int ApprovedCourses { get; set; }
            public int RejectedCourses { get; set; }
            public int NewStudentRegistrations { get; set; }
            public DateTime LastUpdated { get; set; }
            public List<DB.AuditLog> LatestActivities { get; set; } = new();
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
            public int SystemSettingId { get; set; }

            // Branding
            [Required, StringLength(100)]
            public string PlatformName { get; set; } = "";

            public string? LogoPath { get; set; }

            [Required]
            public string PrimaryColor { get; set; } = "#0d6efd";

            // Email / SMTP
            public string? SmtpHost { get; set; }
            public int SmtpPort { get; set; }
            public string? SenderEmail { get; set; }
            public string? SmtpPassword { get; set; }
            public bool EnableEmailNotification { get; set; }

            // Storage
            [Required]
            public string StorageType { get; set; } = "Local";

            public int MaxUploadSizeMB { get; set; } = 50;

            public string AllowedFileTypes { get; set; } = ".pdf,.jpg,.png";

            // Certificate
            public string? CertificateTemplatePath { get; set; }
        }

        // Role Permissin View Models //
        public class RolePermissionVm
        {
            public string RoleName { get; set; } = "";
            public List<string> Permissions { get; set; } = new();
        }

        // PromoCode View Models //
        public class PromoCodeCreateVm
        {
            [Required]
            public string Code { get; set; } = "";

            [Range(1, 100)]
            public int DiscountPercent { get; set; }

            [Required]
            public DateTime StartDate { get; set; }
            [Required]
            public DateTime ExpiryDate { get; set; }

            [Range(1, 1000)]
            public int MaxUsage { get; set; }
        }

        public class PromoCodeListVm
        {
            public int PromoCodeId { get; set; }
            public string Code { get; set; } = "";
            public int DiscountPercent { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime ExpiryDate { get; set; }
            public bool IsActive { get; set; }
            public int UsedCount { get; set; }
        }

        public class ToggleVm
        {
            public int Id { get; set; }
        }

        // ===============================
        // Auto Grading View Models
        // ===============================

        public class AssessmentSubmissionVM
        {
            public int AssessmentId { get; set; }
            public int StudentId { get; set; }

            public List<QuestionAnswerVM> Answers { get; set; } = new();
        }

        public class QuestionAnswerVM
        {
            public int QuestionId { get; set; }
            public string SelectedAnswer { get; set; } = "";
        }

        public class CreateAssessmentVm
        {
            public int CourseId { get; set; }
            public string Title { get; set; } = "";
            public int PassingMark { get; set; }
            public DateTime DeadLine { get; set; }
            public List<CreateAssessmentQuestionVm> Questions { get; set; } = new();
        }

        public class CreateAssessmentQuestionVm
        {
            public string QuestionDetail { get; set; } = "";
            public string AnswerA { get; set; } = "";
            public string AnswerB { get; set; } = "";
            public string AnswerC { get; set; } = "";
            public string AnswerD { get; set; } = "";
            public string CorrectAnswer { get; set; } = "";
        }


    }
}


