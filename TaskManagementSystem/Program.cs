using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Data;
using TaskManagementSystem.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Entity Framework (configurable provider)
var databaseProvider = builder.Configuration.GetValue<string>("DatabaseProvider");
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (string.Equals(databaseProvider, "Sqlite", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlite(connectionString);
    }
    else
    {
        options.UseSqlServer(connectionString);
    }
});

// Add Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

// Add session restore middleware for persistent login
app.UseMiddleware<TaskManagementSystem.SessionRestoreMiddleware>();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Ensure database exists and apply migrations
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (string.Equals(databaseProvider, "Sqlite", StringComparison.OrdinalIgnoreCase))
    {
        context.Database.EnsureCreated();
    }
    else
    {
        try
        {
            context.Database.Migrate();
        }
        catch (SqlException ex) when (ex.Number == 1801)
        {
            // Database already exists (can happen with parallel startups)
        }
    }

    if (!context.Users.Any(u => u.Username == "admin2"))
    {
        context.Users.Add(new User
        {
            Username = "admin2",
            Email = "admin2@afpms.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin1234"),
            FirstName = "System",
            LastName = "Administrator",
            Role = UserRole.Admin,
            Department = "IT",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        context.SaveChanges();
    }
}

app.Run();
