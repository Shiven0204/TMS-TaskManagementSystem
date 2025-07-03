using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Data;
using TaskManagementSystem.Models;
using BCrypt.Net;
using Microsoft.AspNetCore.Http;

namespace TaskManagementSystem.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
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

        [HttpGet]
        public IActionResult Login()
        {
            if (GetUserId() != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, bool rememberMe)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Username and password are required.";
                return View();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ViewBag.Error = "Invalid username or password.";
                return View();
            }

            // Always set session
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("UserRole", user.Role.ToString());
            HttpContext.Session.SetString("UserFullName", $"{user.FirstName} {user.LastName}");

            // Always set cookies (with expiry if rememberMe, session cookie if not)
            var cookieOptions = new CookieOptions
            {
                Path = "/",
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            };
            if (rememberMe)
            {
                cookieOptions.Expires = DateTimeOffset.UtcNow.AddDays(30);
            }
            Response.Cookies.Append("UserId", user.Id.ToString(), cookieOptions);
            Response.Cookies.Append("Username", user.Username, cookieOptions);
            Response.Cookies.Append("UserRole", user.Role.ToString(), cookieOptions);
            Response.Cookies.Append("UserFullName", $"{user.FirstName} {user.LastName}", cookieOptions);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            // Remove persistent cookies
            Response.Cookies.Delete("UserId");
            Response.Cookies.Delete("Username");
            Response.Cookies.Delete("UserRole");
            Response.Cookies.Delete("UserFullName");
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (GetUserId() != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            Console.WriteLine("Register POST action called"); // Debug log
            if (ModelState.IsValid)
            {
                // Check if username already exists
                if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Username already exists.");
                    return View(model);
                }

                // Check if email already exists
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email already exists.");
                    return View(model);
                }

                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Role = model.Role,
                    Department = model.Department,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                ViewBag.Success = "Registration successful. Please login.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        public async Task<IActionResult> Profile()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login");
            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null) return NotFound();
            return View(user);
        }
    }
} 