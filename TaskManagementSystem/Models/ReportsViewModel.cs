namespace TaskManagementSystem.Models
{
    public class ReportsViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalTasks { get; set; }
        public Dictionary<string, int> TasksByStatus { get; set; } = new();
        public Dictionary<string, int> TasksByCategory { get; set; } = new();
        public Dictionary<string, int> TasksByUser { get; set; } = new();
    }
} 