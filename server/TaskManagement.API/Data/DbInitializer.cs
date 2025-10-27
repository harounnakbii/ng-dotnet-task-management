using TaskManagement.API.Models;
using BCrypt.Net;

namespace TaskManagement.API.Data;

public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        // Check if database has been seeded
        if (context.Users.Any())
        {
            return; // Database has been seeded
        }

        // ===========================
        // CREATE TEST USERS
        // ===========================
        var users = new User[]
        {
            new User
            {
                Id = Guid.NewGuid(),
                Name = "John Doe",
                Email = "john@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
                RegistrationDate = DateTime.UtcNow.AddDays(-30),
                IsActive = true
            },
            new User
            {
                Id = Guid.NewGuid(),
                Name = "Alice Smith",
                Email = "alice@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
                RegistrationDate = DateTime.UtcNow.AddDays(-15),
                IsActive = true
            },
            new User
            {
                Id = Guid.NewGuid(),
                Name = "Bob Johnson",
                Email = "bob@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
                RegistrationDate = DateTime.UtcNow.AddDays(-7),
                IsActive = true
            }
        };

        context.Users.AddRange(users);
        context.SaveChanges();

        // ===========================
        // CREATE TEST TASKS
        // ===========================
        var tasks = new TaskItem[]
        {
            // Tasks for John
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Complete project documentation",
                Description = "Write comprehensive README and API documentation",
                DueDate = DateTime.UtcNow.AddDays(7),
                IsCompleted = false,
                UserId = users[0].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Review pull requests",
                Description = "Review and merge pending PRs from team members",
                DueDate = DateTime.UtcNow.AddDays(2),
                IsCompleted = false,
                UserId = users[0].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Fix bug in authentication",
                Description = "Resolve JWT token expiration issue",
                DueDate = DateTime.UtcNow.AddDays(-1),
                IsCompleted = true,
                UserId = users[0].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-3)
            },

            // Tasks for Alice
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Design new UI components",
                Description = "Create mockups for dashboard redesign",
                DueDate = DateTime.UtcNow.AddDays(10),
                IsCompleted = false,
                UserId = users[1].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Conduct user testing",
                Description = "Run usability tests with 5 participants",
                DueDate = DateTime.UtcNow.AddDays(5),
                IsCompleted = false,
                UserId = users[1].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },

            // Tasks for Bob
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Update dependencies",
                Description = "Upgrade NuGet packages to latest stable versions",
                DueDate = DateTime.UtcNow.AddDays(3),
                IsCompleted = false,
                UserId = users[2].Id,
                CreatedAt = DateTime.UtcNow.AddHours(-12),
                UpdatedAt = DateTime.UtcNow.AddHours(-12)
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Setup CI/CD pipeline",
                Description = "Configure GitHub Actions for automated deployment",
                DueDate = DateTime.UtcNow.AddDays(14),
                IsCompleted = false,
                UserId = users[2].Id,
                CreatedAt = DateTime.UtcNow.AddHours(-6),
                UpdatedAt = DateTime.UtcNow.AddHours(-6)
            }
        };

        context.Tasks.AddRange(tasks);
        context.SaveChanges();

        Console.WriteLine($"Database seeded with {users.Length} users and {tasks.Length} tasks");
    }
}