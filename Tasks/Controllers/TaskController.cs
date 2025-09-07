using System.Security.Claims;
using CloudTaskManager.Data;
using CloudTaskManager.DTO_s;
using CloudTaskManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudTaskManager;

[Authorize]
[ApiController]
[Route("api/tasks")]
public class TaskController(TaskDbContext taskDbContext) : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> CreateTask(CreateTaskDto createTaskDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var task = new TaskItem
        {
            Title = createTaskDto.Title,
            Description = createTaskDto.Description,
            DueDate = createTaskDto.DueDate,
            ParentTaskId = createTaskDto.ParentTaskId,
            Status = Status.Pending,
            BoardId = createTaskDto.BoardId,
            AssignedToUserId = userId
        };

        await taskDbContext.TaskItems.AddAsync(task);
        await taskDbContext.SaveChangesAsync();

        return Ok(new
        {
            task.Id,
            task.Title,
            task.DueDate,
            task.Status,
            task.AssignedToUserId
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetTasks([FromQuery]TaskPagedRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);

        var query = taskDbContext.TaskItems
            .Include(t => t.SubTasks)
            .Where(t => t.ParentTaskId == null);

        if (userRole != "BoardOwner")
        {
            query = query.Where(x => x.AssignedToUserId == userId);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(x => x.Title.Contains(request.Search) || 
                                     x.Description!.Contains(request.Search));
        }

        query = request.SortBy?.ToLower() switch
        {
            "duedate" => request.SortDirection.Equals("desc"
                , StringComparison.CurrentCultureIgnoreCase)
                ? query.OrderByDescending(x => x.DueDate)
                : query.OrderBy(x => x.DueDate),

            "title" => request.SortDirection.Equals("desc"
                , StringComparison.CurrentCultureIgnoreCase)
                ? query.OrderByDescending(x => x.Title)
                : query.OrderBy(x => x.Title),

            _ => query.OrderBy(x => x.Id) 
        };

        var totalCount = await query.CountAsync();
        var tasks = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return Ok(new { totalCount, tasks });
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateTask(UpdateTaskDto updateTaskDto)
    {
        var task = await taskDbContext.TaskItems.FindAsync(updateTaskDto.Id);
        if (task == null)
            return NotFound("Task not found");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);

        if (task.AssignedToUserId != userId && userRole != "BoardOwner")
            return Forbid();

        if (!string.IsNullOrEmpty(updateTaskDto.Title))
            task.Title = updateTaskDto.Title;

        if (!string.IsNullOrEmpty(updateTaskDto.Description))
            task.Description = updateTaskDto.Description;

        if (updateTaskDto.DueDate.HasValue)
            task.DueDate = updateTaskDto.DueDate.Value;

        if (updateTaskDto.Status.HasValue)
            task.Status = updateTaskDto.Status.Value;
        
        if (updateTaskDto.BoardId.HasValue)
            task.BoardId = updateTaskDto.BoardId.Value;
        
        if (updateTaskDto.ParentTaskId.HasValue)
            task.ParentTaskId = updateTaskDto.ParentTaskId.Value;

        task.UpdatedAt = DateTime.UtcNow;

        await taskDbContext.SaveChangesAsync();

        return Ok(new { task.Id, Message = "Task updated successfully" });
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        var task = await taskDbContext.TaskItems.FindAsync(id);
        if (task == null)
            return NotFound("Task not found");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);

        if (task.AssignedToUserId != userId && userRole != "BoardOwner")
            return Forbid();

        taskDbContext.TaskItems.Remove(task);
        await taskDbContext.SaveChangesAsync();

        return NoContent();
    }
}