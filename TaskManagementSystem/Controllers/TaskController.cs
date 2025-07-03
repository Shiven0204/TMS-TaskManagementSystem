using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Data;
using TaskManagementSystem.Models;

namespace TaskManagementSystem.Controllers
{
    public class TaskController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TaskController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        private string? GetUserId()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userId))
                return userId;
            // Try cookies
            if (Request.Cookies.ContainsKey("UserId"))
            {
                // Restore session from cookies
                HttpContext.Session.SetString("UserId", Request.Cookies["UserId"]!);
                if (Request.Cookies.ContainsKey("Username"))
                    HttpContext.Session.SetString("Username", Request.Cookies["Username"]!);
                if (Request.Cookies.ContainsKey("UserRole"))
                    HttpContext.Session.SetString("UserRole", Request.Cookies["UserRole"]!);
                if (Request.Cookies.ContainsKey("UserFullName"))
                    HttpContext.Session.SetString("UserFullName", Request.Cookies["UserFullName"]!);
                return Request.Cookies["UserId"];
            }
            return null;
        }

        // GET: Task
        public async Task<IActionResult> Index(string searchString, string categoryFilter, string statusFilter)
        {
            var userId = GetUserId();
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var tasksQuery = _context.Tasks
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .Include(t => t.Updates)
                .Include(t => t.Files)
                .AsQueryable();

            // Apply role-based filtering
            if (userRole == "Admin")
            {
                // Admin can see all tasks
            }
            else if (userRole == "TeamLead")
            {
                // Team Lead can see tasks in their department
                var currentUser = await _context.Users.FindAsync(int.Parse(userId));
                if (currentUser != null)
                {
                    tasksQuery = tasksQuery.Where(t => (t.CreatedBy != null && t.CreatedBy.Department == currentUser.Department) || 
                                                      (t.AssignedTo != null && t.AssignedTo.Department == currentUser.Department));
                }
            }
            else
            {
                // Staff Member can only see their assigned tasks and tasks they created
                tasksQuery = tasksQuery.Where(t => t.AssignedToId == int.Parse(userId) || 
                                                  t.CreatedById == int.Parse(userId));
            }

            // Apply filters
            if (!string.IsNullOrEmpty(searchString))
            {
                tasksQuery = tasksQuery.Where(t => t.Title.Contains(searchString) || 
                                                  t.Description.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(categoryFilter))
            {
                if (Enum.TryParse<TaskCategory>(categoryFilter, out var category))
                {
                    tasksQuery = tasksQuery.Where(t => t.Category == category);
                }
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (Enum.TryParse<TaskManagementSystem.Models.TaskStatus>(statusFilter, out var status))
                {
                    tasksQuery = tasksQuery.Where(t => t.Status == status);
                }
            }

            var tasks = await tasksQuery.OrderByDescending(t => t.CreatedAt).ToListAsync();

            ViewBag.SearchString = searchString;
            ViewBag.CategoryFilter = categoryFilter;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.Categories = Enum.GetValues<TaskCategory>();
            ViewBag.Statuses = Enum.GetValues<TaskManagementSystem.Models.TaskStatus>();

            return View(tasks);
        }

        // GET: Task/Create
        public async Task<IActionResult> Create()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.Users = await _context.Users.Where(u => u.IsActive).ToListAsync();
            ViewBag.Categories = Enum.GetValues<TaskCategory>();
            ViewBag.Priorities = Enum.GetValues<TaskPriority>();

            return View();
        }

        // POST: Task/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskItem task, List<IFormFile> files)
        {
            TempData["Error"] = "DEBUG: Create POST action was called.";
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Your session has expired or you are not logged in. Please log in again.";
                return RedirectToAction("Login", "Auth");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    task.CreatedById = int.Parse(userId);
                    task.CreatedAt = DateTime.Now;
                    task.Status = TaskManagementSystem.Models.TaskStatus.Pending;

                    _context.Tasks.Add(task);
                    await _context.SaveChangesAsync();

                    // Handle file uploads
                    if (files != null && files.Any())
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        foreach (var file in files)
                        {
                            if (file.Length > 0)
                            {
                                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                                var filePath = Path.Combine(uploadsFolder, fileName);

                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    await file.CopyToAsync(stream);
                                }

                                var taskFile = new TaskFile
                                {
                                    TaskId = task.Id,
                                    FileName = file.FileName,
                                    ContentType = file.ContentType,
                                    FilePath = fileName,
                                    FileSize = file.Length,
                                    UploadedById = int.Parse(userId)
                                };

                                _context.TaskFiles.Add(taskFile);
                            }
                        }
                        await _context.SaveChangesAsync();
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "An error occurred while saving the task: " + ex.Message;
                }
            }
            else
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                TempData["Error"] = "ModelState is invalid. Errors: " + errors;
            }

            ViewBag.Users = await _context.Users.Where(u => u.IsActive).ToListAsync();
            ViewBag.Categories = Enum.GetValues<TaskCategory>();
            ViewBag.Priorities = Enum.GetValues<TaskPriority>();

            return View(task);
        }

        // GET: Task/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Tasks
                .Include(t => t.Files)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                return NotFound();
            }

            ViewBag.Users = await _context.Users.Where(u => u.IsActive).ToListAsync();
            ViewBag.Categories = Enum.GetValues<TaskCategory>();
            ViewBag.Priorities = Enum.GetValues<TaskPriority>();
            ViewBag.Statuses = Enum.GetValues<TaskManagementSystem.Models.TaskStatus>();

            return View(task);
        }

        // POST: Task/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaskItem task, List<IFormFile> files)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (id != task.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingTask = await _context.Tasks.FindAsync(id);
                    if (existingTask == null)
                    {
                        return NotFound();
                    }

                    existingTask.Title = task.Title;
                    existingTask.Description = task.Description;
                    existingTask.Category = task.Category;
                    existingTask.Priority = task.Priority;
                    existingTask.Deadline = task.Deadline;
                    existingTask.AssignedToId = task.AssignedToId;
                    existingTask.Status = task.Status;

                    if (task.Status == TaskManagementSystem.Models.TaskStatus.Done && existingTask.CompletedAt == null)
                    {
                        existingTask.CompletedAt = DateTime.Now;
                    }

                    _context.Update(existingTask);

                    // Handle file uploads
                    if (files != null && files.Any())
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        foreach (var file in files)
                        {
                            if (file.Length > 0)
                            {
                                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                                var filePath = Path.Combine(uploadsFolder, fileName);

                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    await file.CopyToAsync(stream);
                                }

                                var taskFile = new TaskFile
                                {
                                    TaskId = task.Id,
                                    FileName = file.FileName,
                                    ContentType = file.ContentType,
                                    FilePath = fileName,
                                    FileSize = file.Length,
                                    UploadedById = int.Parse(userId)
                                };

                                _context.TaskFiles.Add(taskFile);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskExists(task.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                TempData["Error"] = "ModelState is invalid. Errors: " + errors;
            }

            ViewBag.Users = await _context.Users.Where(u => u.IsActive).ToListAsync();
            ViewBag.Categories = Enum.GetValues<TaskCategory>();
            ViewBag.Priorities = Enum.GetValues<TaskPriority>();
            ViewBag.Statuses = Enum.GetValues<TaskManagementSystem.Models.TaskStatus>();

            return View(task);
        }

        // GET: Task/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Tasks
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .Include(t => t.Updates.OrderByDescending(u => u.CreatedAt))
                .ThenInclude(u => u.User)
                .Include(t => t.Files)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        // POST: Task/AddUpdate
        [HttpPost]
        public async Task<IActionResult> AddUpdate(int taskId, string updateText)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(updateText))
            {
                return Json(new { success = false, message = "Invalid request" });
            }

            var taskUpdate = new TaskUpdate
            {
                TaskId = taskId,
                UserId = int.Parse(userId),
                UpdateText = updateText,
                CreatedAt = DateTime.Now
            };

            _context.TaskUpdates.Add(taskUpdate);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // POST: Task/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task != null)
            {
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Task/Download/5
        public async Task<IActionResult> Download(int id)
        {
            var taskFile = await _context.TaskFiles.FindAsync(id);
            if (taskFile == null)
            {
                return NotFound();
            }

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", taskFile.FilePath);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, taskFile.ContentType, taskFile.FileName);
        }

        private bool TaskExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }
    }
} 