using System.ComponentModel.DataAnnotations;

namespace RSD_E_Learning.Models.ViewModels
{
    public class StudentViewModel
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
}
