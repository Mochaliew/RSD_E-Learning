using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using RSD_E_Learning.Models;


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
    public DbSet<SystemSetting> SystemSettings { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<PromoCode> PromoCodes { get; set; }
    public DbSet<CourseFile> CourseFiles { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<Certificate> Certificates { get; set; }
    public DbSet<Assessment> Assessments { get; set; }
    public DbSet<AssessmentSubmission> AssessmentSubmissions { get; set; }

    public DbSet<AssessmentQuestion> AssessmentQuestions { get; set; }

    public DbSet<AssessmentAttempt> AssessmentAttempts { get; set; }
    public DbSet<StudentAnswer> StudentAnswers { get; set; }

    public DbSet<StudentCourseProgress> StudentCourseProgresses { get; set; }
    public DbSet<StudentMaterialProgress> StudentMaterialProgresses { get; set; }


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
        public bool IsDeleted { get; set; }

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

    // ----------------------------------- SYSTEM SETTINGS ------------------------------------ //
    public class SystemSetting
    {
        [Key]
        public int SystemSettingId { get; set; }

        // Platform Branding
        [Required, StringLength(100)]
        public string PlatformName { get; set; } = "RSD E-Learning";

        public string? LogoPath { get; set; }

        [Required]
        public string PrimaryColor { get; set; } = "#0d6efd";

        // SMTP / Email
        public string? SmtpHost { get; set; }
        public int SmtpPort { get; set; } = 587;
        public string? SmtpPassword { get; set; }
        public string? SenderEmail { get; set; }
        public bool EnableEmailNotification { get; set; } = true;

        // Content Storage
        public string StorageType { get; set; } = "Local"; // Local / Cloud
        public int MaxUploadSizeMB { get; set; } = 50;
        public string AllowedFileTypes { get; set; } = ".pdf,.mp4,.docx";

        // Certificate
        public string? CertificateTemplatePath { get; set; }
    }

    // ----------------------------------- TRANSACTION ------------------------------------ //
    public class Transaction
    {
        public int TransactionId { get; set; }

        public int StudentId { get; set; }
        public int CourseId { get; set; }

        public decimal Amount { get; set; }
        public DateTime PaidAt { get; set; }

        public string PaymentMethod { get; set; } = "Manual";
        public string Status { get; set; } = "Paid"; // Paid / Pending / Failed

        public Student Student { get; set; }
        public Course Course { get; set; }
    }

    // ----------------------------------- PROMOCODE ------------------------------------ //
    public class PromoCode
    {
        [Key]
        public int PromoCodeId { get; set; }

        [Required, StringLength(50)]
        public string Code { get; set; } = "";

        [Range(0, 100)]
        public int DiscountPercent { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }

        public bool IsActive { get; set; } = true;

        public int MaxUsage { get; set; } = 100;
        public int UsedCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // ----------------------------------- ENROLLMENT ------------------------------------ //
    public class Enrollment
    {
        [Key]
        public int EnrollmentId { get; set; }

        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
        public bool PaymentStatus { get; set; }
        public string PaymentMethod { get; set; } = "";
        public decimal AmountPaid { get; set; }
    }


    // ----------------------------------- COURSE ------------------------------------ //
    public class Course
    {
        [Key]
        public int CourseId { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; } = "";

        public string? Description { get; set; }

        public decimal Price { get; set; } = 0.0M;

        public int CategoryId { get; set; }
        public int TeacherId { get; set; }
        public bool IsApproved { get; set; } = false;
        public bool IsPublished { get; set; } = false;
        public bool IsRejected { get; set; } = false;
        [StringLength(500)]
        public string? RejectionReason { get; set; }


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

        public int LessonId { get; set; }

        public string FileType { get; set; } = "";

        [Required, StringLength(100)]
        public string FileName { get; set; } = "";

        [Required, StringLength(200)]
        public string FilePath { get; set; } = "";

        public DateTime UpdateAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;  

        // Navigation Properties
        public Lesson? Lesson { get; set; }
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
        public string MeetLink { get; set; } = "";

        public string Description { get; set; } = "";

        public DateTime? ScheduleDate { get; set; }

        // Navigation Properties
        public Course? Course { get; set; }
        
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

    // ----------------------------------- ASSESSMENT ATTEMPT ------------------------------------ //
    public class AssessmentAttempt
    {
        [Key]
        public int AttemptId { get; set; }

        [ForeignKey(nameof(Student))]
        public int StudentId { get; set; }

        [ForeignKey(nameof(Assessment))]
        public int AssessmentId { get; set; }

        public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

        public double Score { get; set; }

        public bool IsPassed { get; set; }

        // Navigation
        public Student? Student { get; set; }
        public Assessment? Assessment { get; set; }

        public ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();
    }

    // ----------------------------------- STUDENT ANSWER ------------------------------------ //
    public class StudentAnswer
    {
        [Key]
        public int AnswerId { get; set; }

        [ForeignKey(nameof(AssessmentAttempt))]
        public int AttemptId { get; set; }

        [ForeignKey(nameof(AssessmentQuestion))]
        public int QuestionId { get; set; }

        [Required]
        public string SelectedAnswer { get; set; } = "";

        public bool IsCorrect { get; set; }

        // Navigation
        public AssessmentAttempt? AssessmentAttempt { get; set; }
        public AssessmentQuestion? Question { get; set; }
    }

    // ----------------------------------- STUDENT COURSE PROGRESS ------------------------------------ //
    public class StudentCourseProgress
    {
        [Key]
        public int StudentCourseProgressId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Range(0, 100)]
        public int ProgressPercentage { get; set; } = 0;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


        // Navigation properties
        public Student? Student { get; set; }
        public Course? Course { get; set; }
    }

    public class StudentMaterialProgress
    {
        [Key]
        public int StudentMaterialProgressId { get; set; }

        public int StudentId { get; set; }
        public int CourseFileId { get; set; }

        public bool IsCompleted { get; set; } = true;

        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Student? Student { get; set; }
        public CourseFile? CourseFile { get; set; }
    }







    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<StudentAnswer>()
        .HasOne(sa => sa.AssessmentAttempt)
        .WithMany(a => a.StudentAnswers)
        .HasForeignKey(sa => sa.AttemptId)
        .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StudentAnswer>()
        .HasOne(sa => sa.Question)
        .WithMany()
        .HasForeignKey(sa => sa.QuestionId)
        .OnDelete(DeleteBehavior.Restrict);

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
        modelBuilder.Entity<Enrollment>()
         .HasOne(e => e.Student)
         .WithMany(s => s.Enrollments)
         .HasForeignKey(e => e.StudentId)
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