using System.ComponentModel.DataAnnotations;

namespace TaskManagementSystem.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        public string? Department { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
        public virtual ICollection<TaskItem> CreatedTasks { get; set; } = new List<TaskItem>();
        public virtual ICollection<TaskUpdate> TaskUpdates { get; set; } = new List<TaskUpdate>();
    }

    public enum UserRole
    {
        Admin,
        TeamLead,
        StaffMember
    }
} 