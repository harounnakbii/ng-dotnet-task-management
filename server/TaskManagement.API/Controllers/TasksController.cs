using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.API.DTOs;
using TaskManagement.API.Services;
using System.Security.Claims;

namespace TaskManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requiert un JWT valide
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskService taskService, ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    /// <summary>
    /// Récupère les tâches avec filtrage et pagination côté serveur
    /// </summary>
    [HttpGet("paged")]
    [ProducesResponseType(typeof(PagedResultDto<TaskDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<TaskDto>>> GetTasksPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = "dueDate",
        [FromQuery] bool sortDescending = false,
        [FromQuery] bool? isCompleted = null,
        [FromQuery] DateTime? dueDateFrom = null,
        [FromQuery] DateTime? dueDateTo = null,
        [FromQuery] string? searchTerm = null)
    {
        var userId = GetCurrentUserId();

        if (userId == Guid.Empty)
            return Unauthorized();

        var filter = new TaskFilterDto
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDescending = sortDescending,
            IsCompleted = isCompleted,
            DueDateFrom = dueDateFrom,
            DueDateTo = dueDateTo,
            SearchTerm = searchTerm
        };

        var result = await _taskService.GetTasksWithFiltersAsync(userId, filter);

        return Ok(result);
    }

    /// <summary>
    /// Crée une nouvelle tâche pour l'utilisateur connecté
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskDto createTaskDto)
    {
        // 🔑 EXTRACTION DU USER ID depuis le JWT Token
        var userId = GetCurrentUserId();

        if (userId == Guid.Empty)
        {
            _logger.LogWarning("Invalid user ID in token");
            return Unauthorized(new { message = "Invalid user token" });
        }

        try
        {
            // Création de la task avec le userId du token
            var task = await _taskService.CreateTaskAsync(createTaskDto, userId);

            _logger.LogInformation("Task {TaskId} created by user {UserId}", task.Id, userId);

            return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task for user {UserId}", userId);
            return BadRequest(new { message = "Failed to create task" });
        }
    }

    /// <summary>
    /// Récupère toutes les tâches de l'utilisateur connecté
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TaskDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetAllTasks()
    {
        var userId = GetCurrentUserId();

        if (userId == Guid.Empty)
            return Unauthorized();

        // Filtre automatique par userId
        var tasks = await _taskService.GetTasksByUserIdAsync(userId);

        return Ok(tasks);
    }

    /// <summary>
    /// Récupère une tâche par ID (uniquement si elle appartient à l'utilisateur)
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TaskDto>> GetTaskById(Guid id)
    {
        var userId = GetCurrentUserId();

        if (userId == Guid.Empty)
            return Unauthorized();

        var task = await _taskService.GetTaskByIdAsync(id);

        if (task == null)
            return NotFound();

        // 🔒 VÉRIFICATION: La tâche appartient-elle à l'utilisateur ?
        if (task.UserId != userId)
        {
            _logger.LogWarning("User {UserId} attempted to access task {TaskId} owned by {OwnerId}",
                userId, id, task.UserId);
            return Forbid(); // 403 Forbidden
        }

        return Ok(task);
    }

    /// <summary>
    /// Met à jour une tâche (uniquement si elle appartient à l'utilisateur)
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> UpdateTask(Guid id, [FromBody] UpdateTaskDto updateTaskDto)
    {
        var userId = GetCurrentUserId();

        if (userId == Guid.Empty)
            return Unauthorized();

        // Vérifier que la tâche existe et appartient à l'utilisateur
        var existingTask = await _taskService.GetTaskByIdAsync(id);

        if (existingTask == null)
            return NotFound();

        if (existingTask.UserId != userId)
            return Forbid();

        // Mettre à jour
        var updatedTask = await _taskService.UpdateTaskAsync(id, updateTaskDto);

        return Ok(updatedTask);
    }

    /// <summary>
    /// Supprime une tâche (uniquement si elle appartient à l'utilisateur)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        var userId = GetCurrentUserId();

        if (userId == Guid.Empty)
            return Unauthorized();

        var existingTask = await _taskService.GetTaskByIdAsync(id);

        if (existingTask == null)
            return NotFound();

        if (existingTask.UserId != userId)
            return Forbid();

        await _taskService.DeleteTaskAsync(id);

        return NoContent();
    }

    /// <summary>
    /// Toggle le statut de complétion d'une tâche
    /// </summary>
    [HttpPatch("{id}/toggle")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TaskDto>> ToggleTaskCompletion(Guid id)
    {
        var userId = GetCurrentUserId();

        if (userId == Guid.Empty)
            return Unauthorized();

        var existingTask = await _taskService.GetTaskByIdAsync(id);

        if (existingTask == null)
            return NotFound();

        if (existingTask.UserId != userId)
            return Forbid();

        var updatedTask = await _taskService.ToggleTaskCompletionAsync(id);

        return Ok(updatedTask);
    }

    // ========================================
    // MÉTHODE HELPER POUR EXTRAIRE USER ID
    // ========================================
    private Guid GetCurrentUserId()
    {
        // Extraire le claim "sub" (Subject) du JWT
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                          ?? User.FindFirst("sub");

        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Guid.Empty;
        }

        return userId;
    }
}