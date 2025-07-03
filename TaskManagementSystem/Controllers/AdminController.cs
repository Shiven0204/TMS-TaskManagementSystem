using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Data;
using TaskManagementSystem.Models;

namespace TaskManagementSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // List all users
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // GET: Admin/CreateUser
        public IActionResult CreateUser()
        {
            return View();
        }

        // POST: Admin/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(User user)
        {
            if (ModelState.IsValid)
            {
                user.CreatedAt = DateTime.Now;
                user.IsActive = true;
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ManageUsers));
            }
            return View(user);
        }

        // GET: Admin/EditUser/5
        public async Task<IActionResult> EditUser(int? id)
        {
            if (id == null) return NotFound();
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            var model = new EditUserViewModel
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                Department = user.Department
            };
            return View(model);
        }

        // POST: Admin/EditUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, EditUserViewModel model)
        {
            if (id != model.Id) return NotFound();
            if (ModelState.IsValid)
            {
                var existingUser = await _context.Users.FindAsync(id);
                if (existingUser == null) return NotFound();
                existingUser.Username = model.Username;
                existingUser.Email = model.Email;
                existingUser.FirstName = model.FirstName;
                existingUser.LastName = model.LastName;
                existingUser.Role = model.Role;
                existingUser.Department = model.Department;
                _context.Update(existingUser);
                await _context.SaveChangesAsync();
                TempData["Success"] = "User updated successfully.";
                return RedirectToAction(nameof(ManageUsers));
            }
            return View(model);
        }

        // POST: Admin/ToggleActive/5
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            user.IsActive = !user.IsActive;
            _context.Update(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageUsers));
        }

        // POST: Admin/DeleteUser/5
        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageUsers));
        }

        public async Task<IActionResult> Reports()
        {
            var model = new ReportsViewModel();
            model.TotalUsers = await _context.Users.CountAsync();
            model.TotalTasks = await _context.Tasks.CountAsync();
            model.TasksByStatus = await _context.Tasks
                .GroupBy(t => t.Status.ToString())
                .ToDictionaryAsync(g => g.Key, g => g.Count());
            model.TasksByCategory = await _context.Tasks
                .GroupBy(t => t.Category.ToString())
                .ToDictionaryAsync(g => g.Key, g => g.Count());
            model.TasksByUser = await _context.Tasks
                .GroupBy(t => t.CreatedBy != null ? t.CreatedBy.FirstName + " " + t.CreatedBy.LastName : "Unknown")
                .ToDictionaryAsync(g => g.Key, g => g.Count());
            return View(model);
        }

        public IActionResult Settings()
        {
            // For demo: use static config. In real app, use DB or config file.
            var model = new SettingsViewModel
            {
                SiteName = SettingsConfig.SiteName,
                AllowRegistration = SettingsConfig.AllowRegistration
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Settings(SettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                // For demo: update static config. In real app, persist to DB or config file.
                SettingsConfig.SiteName = model.SiteName;
                SettingsConfig.AllowRegistration = model.AllowRegistration;
                TempData["Success"] = "Settings updated successfully.";
                return RedirectToAction(nameof(Settings));
            }
            return View(model);
        }
    }
} 