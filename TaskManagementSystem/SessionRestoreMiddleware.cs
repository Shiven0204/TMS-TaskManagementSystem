using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace TaskManagementSystem
{
    public class SessionRestoreMiddleware
    {
        private readonly RequestDelegate _next;
        public SessionRestoreMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Session.GetString("UserId") == null && context.Request.Cookies.ContainsKey("UserId"))
            {
                context.Session.SetString("UserId", context.Request.Cookies["UserId"]!);
                if (context.Request.Cookies.ContainsKey("Username"))
                    context.Session.SetString("Username", context.Request.Cookies["Username"]!);
                if (context.Request.Cookies.ContainsKey("UserRole"))
                    context.Session.SetString("UserRole", context.Request.Cookies["UserRole"]!);
                if (context.Request.Cookies.ContainsKey("UserFullName"))
                    context.Session.SetString("UserFullName", context.Request.Cookies["UserFullName"]!);
            }
            await _next(context);
        }
    }
} 