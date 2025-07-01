using System.ComponentModel.DataAnnotations;

namespace TaskManagementSystem.Models
{
    public class TaskFile
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ContentType { get; set; } = string.Empty;

        [Required]
        public string FilePath { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public DateTime UploadedAt { get; set; }

        // Foreign keys
        public int TaskId { get; set; }
        public int UploadedById { get; set; }

        // Navigation properties
        public virtual TaskItem Task { get; set; } = null!;
        public virtual User UploadedBy { get; set; } = null!;
    }
} 