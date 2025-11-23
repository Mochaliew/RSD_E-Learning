using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSD_E_Learning.Models
{
    public class Teacher
    {
        [Key]
        public int TeacherId { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public User? User { get; set; }

        [Required, StringLength(100)]
        public string SubjectArea { get; set; } = "";

        public bool IsActive { get; set; } = true;
    }
}
