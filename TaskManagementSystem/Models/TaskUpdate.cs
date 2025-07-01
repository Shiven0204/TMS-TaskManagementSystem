using System.ComponentModel.DataAnnotations;

namespace TaskManagementSystem.Models
{
    public class TaskUpdate
    {
        public int Id { get; set; }

        [Required]
        public string UpdateText { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        // Foreign keys
        public int TaskId { get; set; }
        public int UserId { get; set; }

        // Navigation properties
        public virtual TaskItem Task { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
} 