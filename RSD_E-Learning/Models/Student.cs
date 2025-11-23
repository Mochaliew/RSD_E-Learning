using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSD_E_Learning.Models
{
    public class Student
    {
        [Key]
        public int StudentId { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public User? User { get; set; }

        [StringLength(50)]
        public string? ClassName { get; set; }

        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
    }
}
