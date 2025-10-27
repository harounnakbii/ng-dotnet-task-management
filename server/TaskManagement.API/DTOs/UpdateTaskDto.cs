using System.ComponentModel.DataAnnotations;

namespace TaskManagement.API.DTOs;

/// <summary>
/// DTO pour mettre à jour une tâche existante
/// </summary>
public class UpdateTaskDto
{
    [StringLength(200, MinimumLength = 1)]
    public string? Title { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public bool? IsCompleted { get; set; }
}
