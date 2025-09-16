using System.Security.Claims;
using CloudTaskManager.Data;
using CloudTaskManager.DTO_s;
using CloudTaskManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CloudTaskManager.Shared.Correlation;

namespace CloudTaskManager;

[Authorize]
[ApiController]
[Route("api/comments")]
public class CommentController(
    TaskDbContext taskDbContext,
    ILogger<CommentController> logger,
    ICorrelationIdAccessor correlationIdAccessor) : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> CreateComment(CreateCommentDto dto)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning(
                $"Invalid request to create comment [CorrelationId:{correlationIdAccessor.CorrelationId}]");
            return BadRequest(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var task = await taskDbContext.TaskItems.FindAsync(dto.TaskItemId);
        if (task == null)
        {
            logger.LogWarning(
                $"Task not found while creating comment [CorrelationId:{correlationIdAccessor.CorrelationId}]");
            return NotFound("Task not found");
        }

        var comment = new Comment
        {
            Content = dto.Content,
            TaskItemId = dto.TaskItemId,
            UserId = userId
        };

        await taskDbContext.Comments.AddAsync(comment);
        await taskDbContext.SaveChangesAsync();

        logger.LogInformation(
            $"Comment created {comment.Content} [CorrelationId:{correlationIdAccessor.CorrelationId}]");
        return Ok(new { comment.Id, comment.Content, comment.CreatedAt });
    }

    [HttpGet("task/{taskId}")]
    public async Task<IActionResult> GetComments(Guid taskId)
    {
        var comments = await taskDbContext.Comments
            .Where(c => c.TaskItemId == taskId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        logger.LogInformation(
            $"Comments retrieved for {comments.Count} tasks [CorrelationId:{correlationIdAccessor.CorrelationId}]");
        return Ok(comments);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateComment(UpdateCommentDto dto)
    {
        var comment = await taskDbContext.Comments.FindAsync(dto.Id);
        if (comment == null)
        {
            logger.LogWarning(
                $"Comment not found while updating comment [Id:{dto.Id}] [CorrelationId:{correlationIdAccessor.CorrelationId}]");
            return NotFound("Comment not found");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (comment.UserId != userId)
        {
            logger.LogWarning(
                $"Unauthorized to update comment [Id:{dto.Id}] [UserId:{userId}] [CorrelationId:{correlationIdAccessor.CorrelationId}]");
            return Forbid();
        }

        comment.Content = dto.Content;
        await taskDbContext.SaveChangesAsync();

        logger.LogInformation(
            $"Comment Update for {comment.Content} [CorrelationId:{correlationIdAccessor.CorrelationId}]");
        return Ok(new { comment.Id, Message = "Comment updated" });
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteComment(Guid id)
    {
        var comment = await taskDbContext.Comments.FindAsync(id);
        if (comment == null)
        {
            logger.LogWarning(
                $"Comment not found while deleting comment [Id:{id}] [CorrelationId:{correlationIdAccessor.CorrelationId}]");
            return NotFound("Comment not found");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (comment.UserId != userId)
        {
            logger.LogWarning(
                $"Unauthorized to delete comment [Id:{id}] [UserId:{userId}] [CorrelationId:{correlationIdAccessor.CorrelationId}]");
            return Forbid();
        }

        taskDbContext.Comments.Remove(comment);
        await taskDbContext.SaveChangesAsync();

        logger.LogInformation(
            $"Comment Deleted for {comment.Content} [CorrelationId:{correlationIdAccessor.CorrelationId}]");
        return NoContent();
    }
}