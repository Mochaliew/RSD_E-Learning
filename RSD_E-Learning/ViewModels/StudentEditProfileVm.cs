using System.ComponentModel.DataAnnotations;

namespace RSD_E_Learning.ViewModels
{
    public class StudentEditProfileVm
    {
        public int StudentId { get; set; }

        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";

        public string? ClassName { get; set; }
    }
}

