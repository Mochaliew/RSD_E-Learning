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
    public class StudentCreateVm
    {
        [Required, StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

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

    // Course File View Models //
    public class CourseFileVm
    {
        public int CourseId { get; set; }
        public int TeacherId { get; set; }

        [Required]
        public string FileName { get; set; } = "";

        public DateTime UploadedAt { get; set; }

        public bool IsDisabled { get; set; }
    }

    public class LoginViewModel
    {
        [Required, EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = "";

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = "";

        public bool RememberMe { get; set; }
    }

    public class ResetPasswordVm
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";
    }
}
