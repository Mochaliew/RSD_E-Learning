using System;
using System.ComponentModel.DataAnnotations;

namespace RSD_E_Learning.Models
{
    public enum UserRole { Admin, Teacher, Student }

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
    }
}

