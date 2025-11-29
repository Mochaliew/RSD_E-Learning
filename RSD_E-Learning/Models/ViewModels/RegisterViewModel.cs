using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RSD_E_Learning.Models.ViewModels
{
    
    public class RegisterViewModel
    {
        [Required, Display(Name ="Full name"), StringLength(100)]
        public string FullName { get; set;} = " ";

        [Required, EmailAddress, Display(Name = "Email")]
        public string Email { get; set; } = "";

        [Required, DataType(DataType.Password), MinLength(6)]
        public string Password { get; set; } = "";

        [Required, DataType(DataType.Password), Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = "";

    }
}