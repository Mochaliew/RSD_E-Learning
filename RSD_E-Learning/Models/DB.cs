using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSD_E_Learning.Models;

public class DB(DbContextOptions options) : DbContext(options)
{
    public DbSet<Teacher> Teachers { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<Student> Students { get; set; }

    public DbSet<Category> Categories { get; set; }

    public DbSet<AuditLog> AuditLogs { get; set; }

    public DbSet<Course> Courses { get; set; }

    public DbSet<Enrollment> Enrollments { get; set; }

    public DbSet<CourseFile> CourseFiles { get; set; }


    // ----------------------------------- classes ------------------------------------ //


    public enum UserRole { Admin, Teacher, Student }

    public class User
    {
        public int Id { get; set; } // PK

        [Required, StringLength(100)]
        public string FullName { get; set; } = "";

        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; } = "";

        [Required]
        public string PasswordHash { get; set; } = "";

        [Required]
        public UserRole Role { get; set; }

        public int FailedLoginCount { get; set; } = 0;

        public DateTime? LockoutEnd { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Teacher
    {
        [Key]
        public int TeacherId { get; set; } // PK

        [ForeignKey(nameof(User))]
        public int UserId { get; set; } // FK
        public User? User { get; set; }

        [Required, StringLength(100)]
        public string SubjectArea { get; set; } = "";

        public bool IsActive { get; set; } = true;

    }

    public class Student
    {
        [Key]
        public int StudentId { get; set; } // PK

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public User? User { get; set; }

        [StringLength(50)]
        public string? ClassName { get; set; }

        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
    }

    public class Category
    {
        public int CategoryId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = "";

        [StringLength(250)]
        public string? Description { get; set; }
    }

    public class AuditLog
    {
        public int AuditLogId { get; set; } // PK
        public int? UserId { get; set; } // FK 
        public string Action { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Details { get; set; }

    }

    public class Course
    {
        
    }

    public class Enrollment
    {

    }

    public class CourseFile
    {

    }


}
