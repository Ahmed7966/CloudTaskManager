using CloudTaskManager.Models;

namespace CloudTaskManager.DTO_s;

public class TaskPagedRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "DueDate"; 
    public string SortDirection { get; set; } = "asc"; 
    public string? Search { get; set; } = "";
    public Status? Status { get; set; }
}