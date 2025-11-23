using System.ComponentModel.DataAnnotations;

namespace RSD_E_Learning.Models.ViewModels
{
    public class TeacherViewModel
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
}
