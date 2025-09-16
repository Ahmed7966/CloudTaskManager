using System.Security.Claims;
using CloudTaskManager.Data;
using CloudTaskManager.DTO_s;
using CloudTaskManager.Message;
using CloudTaskManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CloudTaskManager.Shared.Correlation;

namespace CloudTaskManager;

[Authorize]
[ApiController]
[Route("api/tasks")]
public class TaskController(
    TaskDbContext taskDbContext,
    IEventPublisher eventPublisher,
    ILogger<TaskController> logger,
    ICorrelationIdAccessor correlationIdAccessor) : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> CreateTask(CreateTaskDto createTaskDto)
    {
        if (!ModelState.IsValid)
        {
            logger.LogError($"Invalid request to create task. [CorrelationId: {correlationIdAccessor.CorrelationId}]");
            return BadRequest(ModelState);
        }

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

        if (createTaskDto.Attachments?.Any() == true)
        {
            task.Attachments = createTaskDto.Attachments.Select(a => new Attachment
            {
                FileUrl = a.FileUrl,
                FileName = a.FileName
            }).ToList();
        }

        if (createTaskDto.Comments?.Any() == true)
        {
            task.Comments = createTaskDto.Comments.Select(c => new Comment
            {
                Content = c.Content,
                UserId = userId
            }).ToList();
        }

        if (createTaskDto.Labels?.Any() == true)
        {
            task.Labels = createTaskDto.Labels.Select(l => new Label
            {
                Name = l.Name,
                Color = l.Color
            }).ToList();
        }

        if (createTaskDto.Reminders?.Any() == true)
        {
            task.Reminders = createTaskDto.Reminders.Select(r => new Reminder
            {
                ReminderTime = r.ReminderTime
            }).ToList();
        }

        await taskDbContext.TaskItems.AddAsync(task);
        await taskDbContext.SaveChangesAsync();

        await eventPublisher.PublishTaskCreated(task);
        
        logger.LogInformation($"Task created: {task.Title} [CorrelationId: {correlationIdAccessor.CorrelationId}]");
        return Ok(new TaskResponseDto
        {
           Id = task.Id,
           Title = task.Title,
           DueDate = task.DueDate,
           Status = task.Status,
           AssignedToUserId = task.AssignedToUserId
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetTasks([FromQuery]TaskPagedRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);

        var query = taskDbContext.TaskItems
            .Include(t => t.SubTasks)
            .Include(t => t.Attachments)
            .Include(t => t.Comments)
            .Include(t => t.Labels)
            .Include(t => t.Reminders)
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
            "duedate" => request.SortDirection.Equals("desc", StringComparison.CurrentCultureIgnoreCase)
                ? query.OrderByDescending(x => x.DueDate)
                : query.OrderBy(x => x.DueDate),

            "title" => request.SortDirection.Equals("desc", StringComparison.CurrentCultureIgnoreCase)
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
        if (!ModelState.IsValid)
        {
            logger.LogError($"Invalid request to update task. [CorrelationId: {correlationIdAccessor.CorrelationId}]");
            return BadRequest(ModelState);
        }
        var task = await taskDbContext.TaskItems
            .Include(t => t.Attachments)
            .Include(t => t.Comments)
            .Include(t => t.Labels)
            .Include(t => t.Reminders)
            .FirstOrDefaultAsync(t => t.Id == updateTaskDto.Id);

        if (task == null)
        {
            logger.LogError($"Task with id: {updateTaskDto.Id} does not exist [CorrelationId: {correlationIdAccessor.CorrelationId}]");
            return NotFound("Task not found");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);

        if (task.AssignedToUserId != userId && userRole != "BoardOwner")
        {
            logger.LogError($"Unauthorized access to task with id: {updateTaskDto.Id} [{userId}] [CorrelationId: {correlationIdAccessor.CorrelationId}]");
            return Forbid();
        }

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

        if (updateTaskDto.Attachments?.Any() == true)
        {
            task.Attachments.Clear();
            foreach (var a in updateTaskDto.Attachments)
            {
                task.Attachments.Add(new Attachment
                {
                    FileUrl = a.FileUrl,
                    FileName = a.FileName
                });
            }
        }

        if (updateTaskDto.Comments?.Any() == true)
        {
            task.Comments.Clear();
            foreach (var c in updateTaskDto.Comments)
            {
                task.Comments.Add(new Comment
                {
                    Content = c.Content,
                    UserId = userId
                });
            }
        }

        if (updateTaskDto.Labels?.Any() == true)
        {
            task.Labels.Clear();
            foreach (var l in updateTaskDto.Labels)
            {
                task.Labels.Add(new Label
                {
                    Name = l.Name,
                    Color = l.Color
                });
            }
        }

        if (updateTaskDto.Reminders?.Any() == true)
        {
            task.Reminders.Clear();
            foreach (var r in updateTaskDto.Reminders)
            {
                task.Reminders.Add(new Reminder
                {
                    ReminderTime = r.ReminderTime
                });
            }
        }

        await taskDbContext.SaveChangesAsync();
        await eventPublisher.PublishTaskUpdated(task);
        logger.LogInformation($"Task Updated: {task.Title} [CorrelationId: {correlationIdAccessor.CorrelationId}]");
        return Ok(new { task.Id, Message = "Task updated successfully" });
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        var task = await taskDbContext.TaskItems.FindAsync(id);
        if (task == null)
        {
            logger.LogError($"Task with id: {id} does not exist [CorrelationId: {correlationIdAccessor.CorrelationId}]");
            return NotFound("Task not found");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);

        if (task.AssignedToUserId != userId && userRole != "BoardOwner")
        {
            logger.LogWarning($"UnAuthorized access to delete task [Userid: {userId}] [CorrelationId: {correlationIdAccessor.CorrelationId}]");
            return Forbid();
        }

        taskDbContext.TaskItems.Remove(task);
        await taskDbContext.SaveChangesAsync();
        await eventPublisher.PublishTaskDeleted(id);
        return NoContent();
    }
}
