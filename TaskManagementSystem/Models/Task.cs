using System.ComponentModel.DataAnnotations;

namespace TaskManagementSystem.Models
{
    public class TaskItem
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public TaskCategory Category { get; set; }

        [Required]
        public TaskPriority Priority { get; set; }

        [Required]
        public DateTime Deadline { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        [Required]
        public TaskStatus Status { get; set; } = TaskStatus.Pending;

        // Foreign keys
        public int CreatedById { get; set; }
        public int? AssignedToId { get; set; }

        // Navigation properties
        public virtual User CreatedBy { get; set; } = null!;
        public virtual User? AssignedTo { get; set; }
        public virtual ICollection<TaskUpdate> Updates { get; set; } = new List<TaskUpdate>();
        public virtual ICollection<TaskFile> Files { get; set; } = new List<TaskFile>();
        public virtual ICollection<TaskReminder> Reminders { get; set; } = new List<TaskReminder>();
    }

    public enum TaskCategory
    {
        Academic,
        Technical,
        Marketing,
        Admin
    }

    public enum TaskPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum TaskStatus
    {
        Pending,
        InProgress,
        Done,
        Blocked,
        Stuck
    }
} 