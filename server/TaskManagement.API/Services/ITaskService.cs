using TaskManagement.API.DTOs;

namespace TaskManagement.API.Services;

/// <summary>
/// Interface pour la gestion des tâches
/// </summary>
public interface ITaskService
{
    // ===========================
    // CRUD OPERATIONS
    // ===========================

    /// <summary>
    /// Crée une nouvelle tâche pour un utilisateur
    /// </summary>
    Task<TaskDto> CreateTaskAsync(CreateTaskDto createTaskDto, Guid userId);

    /// <summary>
    /// Récupère une tâche par son ID
    /// </summary>
    Task<TaskDto?> GetTaskByIdAsync(Guid taskId);

    /// <summary>
    /// Récupère toutes les tâches d'un utilisateur
    /// </summary>
    Task<IEnumerable<TaskDto>> GetTasksByUserIdAsync(Guid userId);

    /// <summary>
    /// Récupère les tâches avec filtres
    /// </summary>
    Task<PagedResultDto<TaskDto>> GetTasksWithFiltersAsync(Guid userId, TaskFilterDto filter);

    /// <summary>
    /// Met à jour une tâche existante
    /// </summary>
    Task<TaskDto> UpdateTaskAsync(Guid taskId, UpdateTaskDto updateTaskDto);

    /// <summary>
    /// Supprime une tâche
    /// </summary>
    Task<bool> DeleteTaskAsync(Guid taskId);

    // ===========================
    // SPECIFIC OPERATIONS
    // ===========================

    /// <summary>
    /// Toggle le statut de complétion d'une tâche
    /// </summary>
    Task<TaskDto> ToggleTaskCompletionAsync(Guid taskId);

    /// <summary>
    /// Marque une tâche comme complétée
    /// </summary>
    Task<TaskDto> MarkAsCompletedAsync(Guid taskId);

    /// <summary>
    /// Marque une tâche comme incomplète
    /// </summary>
    Task<TaskDto> MarkAsIncompleteAsync(Guid taskId);

    // ===========================
    // QUERIES
    // ===========================

    /// <summary>
    /// Récupère les tâches en retard d'un utilisateur
    /// </summary>
    Task<IEnumerable<TaskDto>> GetOverdueTasksAsync(Guid userId);

    /// <summary>
    /// Récupère les tâches à venir (prochains X jours)
    /// </summary>
    Task<IEnumerable<TaskDto>> GetUpcomingTasksAsync(Guid userId, int days = 7);

    /// <summary>
    /// Récupère les tâches complétées d'un utilisateur
    /// </summary>
    Task<IEnumerable<TaskDto>> GetCompletedTasksAsync(Guid userId);

    /// <summary>
    /// Recherche des tâches par titre ou description
    /// </summary>
    Task<IEnumerable<TaskDto>> SearchTasksAsync(Guid userId, string searchTerm);

    // ===========================
    // VALIDATION
    // ===========================

    /// <summary>
    /// Vérifie si une tâche appartient à un utilisateur
    /// </summary>
    Task<bool> IsTaskOwnedByUserAsync(Guid taskId, Guid userId);

    /// <summary>
    /// Vérifie si une tâche existe
    /// </summary>
    Task<bool> TaskExistsAsync(Guid taskId);
}