using System.ComponentModel.DataAnnotations;

namespace RSD_E_Learning.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required, EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = "";

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = "";

        public bool RememberMe { get; set; }
    }
}
