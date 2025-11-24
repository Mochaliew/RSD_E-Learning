using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

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

    // ROLE
    public enum UserRole { Admin, Teacher, Student }

    // USER
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

    // TEACHER
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

    // STUDENT
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

    //CATEGORY
    public class Category
    {
        public int CategoryId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = "";

        [StringLength(250)]
        public string? Description { get; set; }
    }

    // AUDITLOG
    public class AuditLog
    {
        public int AuditLogId { get; set; } // PK
        public int? UserId { get; set; } // FK 
        public string Action { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Details { get; set; }

    }

    // COURSE
    public class Course
    {
        public int Id { get; set; } // PK

        [Required, StringLength(100)]
        public string Title { get; set; } = "";

        public string? Description { get; set; }

        [ForeignKey(nameof(Category))]
        public int CategoryId { get; set; } // FK

        [ForeignKey(nameof(Teacher))]
        public int TeacherId { get; set; } // FK
    }

    // ENROLLMENT
    public class Enrollment
    {
        public int EnrollmentId { get; set; } // PK

        public int StudentId { get; set; } // FK

        public int CourseId { get; set; } // FK

        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    }

    // COURSEFILE
    public class CourseFile
    {
        public int CourseFileId { get; set; } // PK

        public int CourseId { get; set; } // FK

        public int TeacherId { get; set; } // FK

        [Required, StringLength(100)]
        public string FileName { get; set; } = "";

        [Required, StringLength(200)]
        public string FilePath { get; set; } = "";

        public DateTime UpdateAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }


}
