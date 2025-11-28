using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace RSD_E_Learning.Models;

public class DB(DbContextOptions options) : DbContext(options)
{

    public DbSet<User> Users { get; set; }

    public DbSet<Teacher> Teachers { get; set; }

    public DbSet<Admin> Admins { get; set; }

    public DbSet<Student> Students { get; set; }

    public DbSet<Category> Categories { get; set; }

    public DbSet<AuditLog> AuditLogs { get; set; }

    public DbSet<Course> Courses { get; set; }

    public DbSet<Enrollment> Enrollments { get; set; }

    public DbSet<CourseFile> CourseFiles { get; set; }

    public DbSet<Lesson> Lessons { get; set; }

    public DbSet<Certificate> Certificates { get; set; }

    public DbSet<Assessment> Assessments { get; set; }

    public DbSet<AssessmentSubmission> AssessmentSubmissions { get; set; }



    // ----------------------------------- classes ------------------------------------ //

    // ROLE
    public enum UserRole { Admin, Teacher, Student }

    // USER
    public class User
    {
        public int Id { get; set; } // PK

        [StringLength(100)]
        public string FullName { get; set; } = "";

        [EmailAddress, StringLength(150)]
        public string Email { get; set; } = "";


        public string PasswordHash { get; set; } = "";


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
        public int UserId { get; set; } // FK

        [StringLength(100)]
        public string SubjectArea { get; set; } = "";

        public bool IsActive { get; set; } = true;

    }

    // ADMIN
    public class Admin
    {
        [Key]
        public int AdminId { get; set; } // PK

        public int UserId { get; set; } // FK

    }

    // STUDENT
    public class Student
    {
        [Key]
        public int StudentId { get; set; } // PK

        public int UserId { get; set; }
    }

    //CATEGORY
    public class Category
    {
        public int CategoryId { get; set; }

        [StringLength(100)]
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

        public string? Details { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    }

    // ENROLLMENT
    public class Enrollment
    {
        public int EnrollmentId { get; set; } // PK

        public int StudentId { get; set; } // FK

        public int CourseId { get; set; } // FK

        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

        public bool EnrollStatus { get; set; } = false;

    }

    // PAYMENT

    public class Payment
    {
        public int PaymentId { get; set; } // PK

        public int EnrollmentId { get; set; } // FK

        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string PaymentMethod { get; set; } = "";

        public bool PaymentStatus { get; set; } = false;

    }

    // COURSE
    public class Course
    {
        public int Id { get; set; } // PK

        public int CategoryId { get; set; } // FK

        public int TeacherId { get; set; } // FK

        [StringLength(100)]
        public string Title { get; set; } = "";

        public string? Description { get; set; }

    }

    // COURSEFILE
    public class CourseFile
    {
        public int CourseFileId { get; set; } // PK

        public int CourseId { get; set; } // FK

        public int TeacherId { get; set; } // FK

        [StringLength(100)]
        public string FileName { get; set; } = "";

        [Required, StringLength(200)]
        public string FilePath { get; set; } = "";

        public DateTime UpdateAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }

    // LESSON
    public class Lesson
    {
        public int LessonId { get; set; } // PK

        public int CourseId { get; set; } // FK

        [StringLength(100)]
        public string Title { get; set; } = "";

        [StringLength(200)]
        public string PDFFilePath { get; set; } = "";

        [StringLength(200)]
        public string VideoURL { get; set; } = "";

        public string? LessonContent { get; set; }
    }

    // CERTIFICATE
    public class Certificate
    {
        public int CertificateId { get; set; } // PK

        public int StudentId { get; set; } // FK

        public int CourseId { get; set; } // FK

        public string Title { get; set; } = "";

        public DateTime IssuedDate { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string CertificateURL { get; set; } = "";
    }

    // ASSESSMENT
    public class Assessment
    {
        public int AssessmentId { get; set; } // PK

        public int CourseId { get; set; } // FK

        [StringLength(200)]
        public string Title { get; set; } = "";

        public int? TotalMarks { get; set; }

        public DateTime DeadLine { get; set; }
    }

    // ASSESSMENT SUBMISSION
    public class AssessmentSubmission
    {
        public int SubmissionId { get; set; } // PK

        public int StudentId { get; set; } // FK

        public int AssessmentId { get; set; } // FK

        [StringLength(200)]
        public string FileURL { get; set; } = "";

        public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;

        public double? Score { get; set; }

        public string? Feedback { get; set; }
    }


}
