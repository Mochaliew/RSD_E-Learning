using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RSD_E_Learning.Models;

public class DB : DbContext
{
    public DB(DbContextOptions<DB> options) : base(options)
    {
    }
      



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

    public DbSet<AssessmentQuestion> AssessmentQuestions { get; set; }

    // ----------------------------------- ROLE ENUM ------------------------------------ //
    public enum UserRole { Admin, Teacher, Student }

    // ----------------------------------- USER ------------------------------------ //
    public class User
    {
        public int Id { get; set; }

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

        // Navigation Properties
        public Teacher? Teacher { get; set; }
        public Admin? Admin { get; set; }
        public Student? Student { get; set; }
    }

    // ----------------------------------- TEACHER ------------------------------------ //
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

        // Navigation Properties
        public ICollection<Course> Courses { get; set; } = new List<Course>();
        public ICollection<CourseFile> CourseFiles { get; set; } = new List<CourseFile>();
    }

    // ----------------------------------- ADMIN ------------------------------------ //
    public class Admin
    {
        [Key]
        public int AdminId { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        public User? User { get; set; }
    }

    // ----------------------------------- STUDENT ------------------------------------ //
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

        // Navigation Properties
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
        public ICollection<AssessmentSubmission> AssessmentSubmissions { get; set; } = new List<AssessmentSubmission>();
    }

    // ----------------------------------- CATEGORY ------------------------------------ //
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = "";

        [StringLength(250)]
        public string? Description { get; set; }

        // Navigation Properties
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }

    // ----------------------------------- AUDITLOG ------------------------------------ //
    public class AuditLog
    {
        [Key]
        public int AuditLogId { get; set; }

        public int? UserId { get; set; }

        public string Action { get; set; } = "";

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string? Details { get; set; }
    }

    // ----------------------------------- ENROLLMENT ------------------------------------ //
    public class Enrollment
    {
        [Key]
        public int EnrollmentId { get; set; }

        [ForeignKey(nameof(Student))]
        public int StudentId { get; set; }

        [ForeignKey(nameof(Course))]
        public int CourseId { get; set; }

        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

        public bool PaymentStatus { get; set; } = false;

        public string? PaymentReference { get; set; }

        // Navigation Properties
        public Student? Student { get; set; }
        public Course? Course { get; set; }
    }

    // ----------------------------------- COURSE ------------------------------------ //
    public class Course
    {
        [Key]
        public int CourseId { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; } = "";

        public string? Description { get; set; }

        public int CategoryId { get; set; }
        public int TeacherId { get; set; }
        public bool IsApproved { get; set; } = false;
        public bool IsPublished { get; set; } = false;


        // Navigation Properties
        public Category? Category { get; set; }
        public Teacher? Teacher { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<CourseFile> CourseFiles { get; set; } = new List<CourseFile>();
        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
        public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
        public ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
    }

    // ----------------------------------- COURSEFILE ------------------------------------ //
    public class CourseFile
    {
        [Key]
        public int CourseFileId { get; set; }

        public int CourseId { get; set; }


        public string FileType { get; set; } = "";

        [Required, StringLength(100)]
        public string FileName { get; set; } = "";

        [Required, StringLength(200)]
        public string FilePath { get; set; } = "";

        public DateTime UpdateAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public Course? Course { get; set; }
    }

    // ----------------------------------- LESSON ------------------------------------ //
    public class Lesson
    {
        [Key]
        public int LessonId { get; set; }

        [ForeignKey(nameof(Course))]
        public int CourseId { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; } = "";

        [Required, StringLength(200)]
        public string FilePath { get; set; } = "";

        public string? Content { get; set; }

        // Navigation Properties
        public Course? Course { get; set; }
        public ICollection<AssessmentSubmission> AssessmentSubmissions { get; set; } = new List<AssessmentSubmission>();
    }

    // ----------------------------------- CERTIFICATE ------------------------------------ //
    public class Certificate
    {
        [Key]
        public int CertificateId { get; set; }

        [ForeignKey(nameof(Student))]
        public int StudentId { get; set; }

        [ForeignKey(nameof(Course))]
        public int CourseId { get; set; }

        public DateTime IssuedDate { get; set; } = DateTime.UtcNow;

        [Required, StringLength(100)]
        public string CertificateURL { get; set; } = "";

        // Navigation Properties
        public Student? Student { get; set; }
        public Course? Course { get; set; }
    }

    // ----------------------------------- ASSESSMENT ------------------------------------ //
    public class Assessment
    {
        [Key]
        public int AssessmentId { get; set; }

        [ForeignKey(nameof(Course))]
        public int CourseId { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = "";

        public int? TotalMarks { get; set; }

        public DateTime DeadLine { get; set; }

        // Navigation Properties
        public Course? Course { get; set; }
        public ICollection<AssessmentSubmission> AssessmentSubmissions { get; set; } = new List<AssessmentSubmission>();
    }

    // ----------------------------------- ASSESSMENT SUBMISSION ------------------------------------ //
    public class AssessmentSubmission
    {
        [Key]
        public int SubmissionId { get; set; }

        [ForeignKey(nameof(Student))]
        public int StudentId { get; set; }

        [ForeignKey(nameof(Lesson))]
        public int LessonId { get; set; }

        public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;

        public double? Grade { get; set; }

        // Navigation Properties
        public Student? Student { get; set; }
        public Lesson? Lesson { get; set; }
    }

    // ----------------------------------- ASSESSMENT QUESTION ------------------------------------ //

    public class AssessmentQuestion
    {
        [Key]
        public int QuestionId { get; set; }
        [ForeignKey(nameof(Assessment))]
        public int AssessmentId { get; set; }

        [Required, StringLength(500)]
        public string QuestionDetail { get; set; } = "";

        public string AnswerA { get; set; } = "";

        public string AnswerB { get; set; } = "";

        public string AnswerC { get; set; } = "";

        public string AnswerD { get; set; } = "";

        public string CorrectAnswer { get; set; } = "";

        public Assessment? Assessment { get; set; }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //Admin Seed//
        var adminUser = new User
        {
            Id = 1,
            FullName = "System Administrator",
            Email = "admin@elearning.com",
            PasswordHash = HashPassword("admin123"),
            Role = UserRole.Admin,
            CreatedAt = new DateTime(2025, 1, 1)
        };

        modelBuilder.Entity<User>().HasData(adminUser);

        modelBuilder.Entity<Admin>().HasData(new Admin
        {
            AdminId = 1,
            UserId = 1
        });

        //Relationships//
        modelBuilder.Entity<Course>()
            .HasOne(c => c.Teacher)
            .WithMany(t => t.Courses)
            .HasForeignKey(c => c.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);


        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Disable cascade delete for Enrollment -> Course relationship
        modelBuilder.Entity<DB.Enrollment>()
            .HasOne<DB.Course>()
            .WithMany()
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
    private static string HashPassword(string password)
    {
        byte[] salt = Encoding.UTF8.GetBytes("STATIC-SALT-CHANGE-LATER");

        return Convert.ToBase64String(
            KeyDerivation.Pbkdf2(
                password,
                salt,
                KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32
            )
        );
    }


}