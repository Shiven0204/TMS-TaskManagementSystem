using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Data;
using TaskManagementSystem.Models;

namespace TaskManagementSystem.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Auth");
        }

        var userRole = HttpContext.Session.GetString("UserRole");
        var currentUserId = int.Parse(userId);

        // Get user's tasks based on role
        var tasksQuery = _context.Tasks
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .AsQueryable();

        if (userRole == "Admin")
        {
            // Admin sees all tasks
        }
        else if (userRole == "TeamLead")
        {
            var currentUser = await _context.Users.FindAsync(currentUserId);
            if (currentUser != null)
            {
                tasksQuery = tasksQuery.Where(t => (t.CreatedBy != null && t.CreatedBy.Department == currentUser.Department) || 
                                                  (t.AssignedTo != null && t.AssignedTo.Department == currentUser.Department));
            }
        }
        else
        {
            // Staff Member sees only their tasks
            tasksQuery = tasksQuery.Where(t => t.AssignedToId == currentUserId || 
                                              t.CreatedById == currentUserId);
        }

        var dashboardData = new DashboardViewModel
        {
            TotalTasks = await tasksQuery.CountAsync(),
            PendingTasks = await tasksQuery.CountAsync(t => t.Status == TaskManagementSystem.Models.TaskStatus.Pending),
            InProgressTasks = await tasksQuery.CountAsync(t => t.Status == TaskManagementSystem.Models.TaskStatus.InProgress),
            CompletedTasks = await tasksQuery.CountAsync(t => t.Status == TaskManagementSystem.Models.TaskStatus.Done),
            OverdueTasks = await tasksQuery.CountAsync(t => t.Deadline < DateTime.Now && t.Status != TaskManagementSystem.Models.TaskStatus.Done),
            RecentTasks = await tasksQuery
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .ToListAsync(),
            UpcomingDeadlines = await tasksQuery
                .Where(t => t.Deadline > DateTime.Now && t.Status != TaskManagementSystem.Models.TaskStatus.Done)
                .OrderBy(t => t.Deadline)
                .Take(5)
                .ToListAsync()
        };

        return View(dashboardData);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
