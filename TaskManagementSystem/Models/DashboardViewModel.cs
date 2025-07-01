using System.Collections.Generic;

namespace TaskManagementSystem.Models
{
    public class DashboardViewModel
    {
        public int TotalTasks { get; set; }
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OverdueTasks { get; set; }
        public List<TaskItem> RecentTasks { get; set; } = new List<TaskItem>();
        public List<TaskItem> UpcomingDeadlines { get; set; } = new List<TaskItem>();
    }
} 