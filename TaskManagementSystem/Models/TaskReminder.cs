using System.ComponentModel.DataAnnotations;

namespace TaskManagementSystem.Models
{
    public class TaskReminder
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Message { get; set; } = string.Empty;

        [Required]
        public DateTime ReminderDate { get; set; }

        public bool IsSent { get; set; } = false;

        public DateTime? SentAt { get; set; }

        public ReminderType Type { get; set; }

        // Foreign keys
        public int TaskId { get; set; }

        // Navigation properties
        public virtual TaskItem Task { get; set; } = null!;
    }

    public enum ReminderType
    {
        Deadline,
        FollowUp,
        Blocked,
        General
    }
} 