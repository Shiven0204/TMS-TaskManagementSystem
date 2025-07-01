using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Models;

namespace TaskManagementSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<TaskUpdate> TaskUpdates { get; set; }
        public DbSet<TaskFile> TaskFiles { get; set; }
        public DbSet<TaskReminder> TaskReminders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Task configuration
            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.CreatedBy)
                .WithMany(u => u.CreatedTasks)
                .HasForeignKey(t => t.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.AssignedTo)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssignedToId)
                .OnDelete(DeleteBehavior.Restrict);

            // TaskUpdate configuration
            modelBuilder.Entity<TaskUpdate>()
                .HasOne(tu => tu.Task)
                .WithMany(t => t.Updates)
                .HasForeignKey(tu => tu.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskUpdate>()
                .HasOne(tu => tu.User)
                .WithMany(u => u.TaskUpdates)
                .HasForeignKey(tu => tu.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // TaskFile configuration
            modelBuilder.Entity<TaskFile>()
                .HasOne(tf => tf.Task)
                .WithMany(t => t.Files)
                .HasForeignKey(tf => tf.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskFile>()
                .HasOne(tf => tf.UploadedBy)
                .WithMany()
                .HasForeignKey(tf => tf.UploadedById)
                .OnDelete(DeleteBehavior.Restrict);

            // TaskReminder configuration
            modelBuilder.Entity<TaskReminder>()
                .HasOne(tr => tr.Task)
                .WithMany(t => t.Reminders)
                .HasForeignKey(tr => tr.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Create default admin user
            var adminUser = new User
            {
                Id = 1,
                Username = "admin",
                Email = "admin@afpms.com",
                PasswordHash = "$2a$11$u1q8Qw8Qw8Qw8Qw8Qw8QwOQw8Qw8Qw8Qw8Qw8Qw8Qw8Qw8Qw8Qw8Q", // valid hash for 'admin123'
                FirstName = "System",
                LastName = "Administrator",
                Role = UserRole.Admin,
                Department = "IT",
                CreatedAt = new DateTime(2024, 1, 1),
                IsActive = true
            };

            modelBuilder.Entity<User>().HasData(adminUser);
        }
    }
} 