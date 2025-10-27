using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Data;
using TaskManagement.API.DTOs;
using TaskManagement.API.Models;

namespace TaskManagement.API.Services;

/// <summary>
/// Service de gestion des tâches
/// </summary>
public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TaskService> _logger;

    public TaskService(ApplicationDbContext context, ILogger<TaskService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ===========================
    // CRUD OPERATIONS
    // ===========================

    public async Task<TaskDto> CreateTaskAsync(CreateTaskDto createTaskDto, Guid userId)
    {
        _logger.LogInformation("Creating task for user {UserId}", userId);

        // Vérifier que l'utilisateur existe
        var userExists = await _context.Users.AnyAsync(u => u.Id == userId && u.IsActive);
        if (!userExists)
        {
            throw new InvalidOperationException($"User {userId} not found or inactive");
        }

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = createTaskDto.Title,
            Description = createTaskDto.Description,
            DueDate = createTaskDto.DueDate.ToUniversalTime(),
            IsCompleted = false,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Task {TaskId} created successfully for user {UserId}", task.Id, userId);

        return await MapToDto(task);
    }

    public async Task<TaskDto?> GetTaskByIdAsync(Guid taskId)
    {
        var task = await _context.Tasks
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        return task != null ? await MapToDto(task) : null;
    }

    public async Task<IEnumerable<TaskDto>> GetTasksByUserIdAsync(Guid userId)
    {
        var tasks = await _context.Tasks
            .Include(t => t.User)
            .Where(t => t.UserId == userId)
            .OrderBy(t => t.DueDate)
            .ToListAsync();

        var taskDtos = new List<TaskDto>();
        foreach (var task in tasks)
        {
            taskDtos.Add(await MapToDto(task));
        }

        return taskDtos;
    }

    public async Task<PagedResultDto<TaskDto>> GetTasksWithFiltersAsync(Guid userId, TaskFilterDto filter)
    {
        var query = _context.Tasks
            .Include(t => t.User)
            .Where(t => t.UserId == userId)
            .AsQueryable();

        // Filtrer par statut de complétion
        if (filter.IsCompleted.HasValue)
        {
            query = query.Where(t => t.IsCompleted == filter.IsCompleted.Value);
        }

        // Filtrer par date de début
        if (filter.DueDateFrom.HasValue)
        {
            query = query.Where(t => t.DueDate >= filter.DueDateFrom.Value.ToUniversalTime());
        }

        // Filtrer par date de fin
        if (filter.DueDateTo.HasValue)
        {
            query = query.Where(t => t.DueDate <= filter.DueDateTo.Value.ToUniversalTime());
        }

        // Recherche textuelle
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchLower = filter.SearchTerm.ToLower();
            query = query.Where(t =>
                t.Title.ToLower().Contains(searchLower) ||
                t.Description.ToLower().Contains(searchLower));
        }

        // Tri
        query = filter.SortBy?.ToLower() switch
        {
            "title" => filter.SortDescending
                ? query.OrderByDescending(t => t.Title)
                : query.OrderBy(t => t.Title),
            "createdat" => filter.SortDescending
                ? query.OrderByDescending(t => t.CreatedAt)
                : query.OrderBy(t => t.CreatedAt),
            "duedate" or _ => filter.SortDescending
                ? query.OrderByDescending(t => t.DueDate)
                : query.OrderBy(t => t.DueDate)
        };

        // Compter le total
        var totalCount = await query.CountAsync();

        // Pagination
        var tasks = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var taskDtos = new List<TaskDto>();
        foreach (var task in tasks)
        {
            taskDtos.Add(await MapToDto(task));
        }

        return new PagedResultDto<TaskDto>
        {
            Items = taskDtos,
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    public async Task<TaskDto> UpdateTaskAsync(Guid taskId, UpdateTaskDto updateTaskDto)
    {
        var task = await _context.Tasks.FindAsync(taskId);

        if (task == null)
        {
            throw new KeyNotFoundException($"Task {taskId} not found");
        }

        // Mettre à jour les champs fournis
        if (!string.IsNullOrEmpty(updateTaskDto.Title))
        {
            task.Title = updateTaskDto.Title;
        }

        if (updateTaskDto.Description != null)
        {
            task.Description = updateTaskDto.Description;
        }

        if (updateTaskDto.DueDate.HasValue)
        {
            task.DueDate = updateTaskDto.DueDate.Value.ToUniversalTime();
        }

        if (updateTaskDto.IsCompleted.HasValue)
        {
            task.IsCompleted = updateTaskDto.IsCompleted.Value;
        }

        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Task {TaskId} updated successfully", taskId);

        return await MapToDto(task);
    }

    public async Task<bool> DeleteTaskAsync(Guid taskId)
    {
        var task = await _context.Tasks.FindAsync(taskId);

        if (task == null)
        {
            return false;
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Task {TaskId} deleted successfully", taskId);

        return true;
    }

    // ===========================
    // SPECIFIC OPERATIONS
    // ===========================

    public async Task<TaskDto> ToggleTaskCompletionAsync(Guid taskId)
    {
        var task = await _context.Tasks.FindAsync(taskId);

        if (task == null)
        {
            throw new KeyNotFoundException($"Task {taskId} not found");
        }

        task.IsCompleted = !task.IsCompleted;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Task {TaskId} completion toggled to {IsCompleted}", taskId, task.IsCompleted);

        return await MapToDto(task);
    }

    public async Task<TaskDto> MarkAsCompletedAsync(Guid taskId)
    {
        var task = await _context.Tasks.FindAsync(taskId);

        if (task == null)
        {
            throw new KeyNotFoundException($"Task {taskId} not found");
        }

        task.IsCompleted = true;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Task {TaskId} marked as completed", taskId);

        return await MapToDto(task);
    }

    public async Task<TaskDto> MarkAsIncompleteAsync(Guid taskId)
    {
        var task = await _context.Tasks.FindAsync(taskId);

        if (task == null)
        {
            throw new KeyNotFoundException($"Task {taskId} not found");
        }

        task.IsCompleted = false;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Task {TaskId} marked as incomplete", taskId);

        return await MapToDto(task);
    }

    // ===========================
    // QUERIES
    // ===========================

    public async Task<IEnumerable<TaskDto>> GetOverdueTasksAsync(Guid userId)
    {
        var now = DateTime.UtcNow;
        var tasks = await _context.Tasks
            .Include(t => t.User)
            .Where(t => t.UserId == userId && !t.IsCompleted && t.DueDate < now)
            .OrderBy(t => t.DueDate)
            .ToListAsync();

        var taskDtos = new List<TaskDto>();
        foreach (var task in tasks)
        {
            taskDtos.Add(await MapToDto(task));
        }

        return taskDtos;
    }

    public async Task<IEnumerable<TaskDto>> GetUpcomingTasksAsync(Guid userId, int days = 7)
    {
        var now = DateTime.UtcNow;
        var futureDate = now.AddDays(days);

        var tasks = await _context.Tasks
            .Include(t => t.User)
            .Where(t => t.UserId == userId && !t.IsCompleted && t.DueDate >= now && t.DueDate <= futureDate)
            .OrderBy(t => t.DueDate)
            .ToListAsync();

        var taskDtos = new List<TaskDto>();
        foreach (var task in tasks)
        {
            taskDtos.Add(await MapToDto(task));
        }

        return taskDtos;
    }

    public async Task<IEnumerable<TaskDto>> GetCompletedTasksAsync(Guid userId)
    {
        var tasks = await _context.Tasks
            .Include(t => t.User)
            .Where(t => t.UserId == userId && t.IsCompleted)
            .OrderByDescending(t => t.UpdatedAt)
            .ToListAsync();

        var taskDtos = new List<TaskDto>();
        foreach (var task in tasks)
        {
            taskDtos.Add(await MapToDto(task));
        }

        return taskDtos;
    }

    public async Task<IEnumerable<TaskDto>> SearchTasksAsync(Guid userId, string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetTasksByUserIdAsync(userId);
        }

        var searchLower = searchTerm.ToLower();
        var tasks = await _context.Tasks
            .Include(t => t.User)
            .Where(t => t.UserId == userId &&
                   (t.Title.ToLower().Contains(searchLower) ||
                    t.Description.ToLower().Contains(searchLower)))
            .OrderBy(t => t.DueDate)
            .ToListAsync();

        var taskDtos = new List<TaskDto>();
        foreach (var task in tasks)
        {
            taskDtos.Add(await MapToDto(task));
        }

        return taskDtos;
    }

    // ===========================
    // VALIDATION
    // ===========================

    public async Task<bool> IsTaskOwnedByUserAsync(Guid taskId, Guid userId)
    {
        return await _context.Tasks.AnyAsync(t => t.Id == taskId && t.UserId == userId);
    }

    public async Task<bool> TaskExistsAsync(Guid taskId)
    {
        return await _context.Tasks.AnyAsync(t => t.Id == taskId);
    }

    // ===========================
    // HELPER METHODS
    // ===========================

    private async Task<TaskDto> MapToDto(TaskItem task)
    {
        if (task.User == null)
        {
            task.User = await _context.Users.FindAsync(task.UserId) ?? new User();
        }

        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            DueDate = task.DueDate,
            IsCompleted = task.IsCompleted,
            UserId = task.UserId,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }
}