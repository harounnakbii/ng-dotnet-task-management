namespace TaskManagement.API.DTOs;

/// <summary>
/// DTO pour les filtres de recherche
/// </summary>
public class TaskFilterDto
{
    public bool? IsCompleted { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; } = "DueDate"; // DueDate, Title, CreatedAt
    public bool SortDescending { get; set; } = false;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}