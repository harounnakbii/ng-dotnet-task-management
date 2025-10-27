using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Models;

namespace TaskManagement.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<TaskItem> Tasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===========================
        // USER CONFIGURATION
        // ===========================
        modelBuilder.Entity<User>(entity =>
        {
            // Primary Key
            entity.HasKey(e => e.Id);

            // Indexes
            entity.HasIndex(e => e.Email)
                  .IsUnique()
                  .HasDatabaseName("IX_Users_Email");

            // Properties
            entity.Property(e => e.Id)
                  .HasDefaultValueSql("NEWID()");

            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(e => e.Email)
                  .IsRequired()
                  .HasMaxLength(256);

            entity.Property(e => e.PasswordHash)
                  .IsRequired()
                  .HasMaxLength(500);

            entity.Property(e => e.RegistrationDate)
                  .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.IsActive)
                  .HasDefaultValue(true);

            // Relationships
            entity.HasMany(e => e.Tasks)
                  .WithOne(t => t.User)
                  .HasForeignKey(t => t.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ===========================
        // TASK CONFIGURATION
        // ===========================
        modelBuilder.Entity<TaskItem>(entity =>
        {
            // Primary Key
            entity.HasKey(e => e.Id);

            // Indexes
            entity.HasIndex(e => e.UserId)
                  .HasDatabaseName("IX_Tasks_UserId");

            entity.HasIndex(e => e.DueDate)
                  .HasDatabaseName("IX_Tasks_DueDate");

            entity.HasIndex(e => e.IsCompleted)
                  .HasDatabaseName("IX_Tasks_IsCompleted");

            // Properties
            entity.Property(e => e.Id)
                  .HasDefaultValueSql("NEWID()");

            entity.Property(e => e.Title)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(e => e.Description)
                  .HasMaxLength(1000);

            entity.Property(e => e.DueDate)
                  .IsRequired();

            entity.Property(e => e.IsCompleted)
                  .HasDefaultValue(false);

            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.UpdatedAt)
                  .HasDefaultValueSql("GETUTCDATE()");

            // Relationships are configured in User entity
        });
    }

    // Override SaveChanges to update UpdatedAt automatically
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is TaskItem &&
                   (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var task = (TaskItem)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                task.CreatedAt = DateTime.UtcNow;
            }

            task.UpdatedAt = DateTime.UtcNow;
        }
    }
}