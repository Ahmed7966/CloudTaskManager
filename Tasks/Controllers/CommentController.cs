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
[Route("api/comments")]
public class CommentController(TaskDbContext taskDbContext) : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> CreateComment(CreateCommentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var task = await taskDbContext.TaskItems.FindAsync(dto.TaskItemId);
        if (task == null) return NotFound("Task not found");

        var comment = new Comment
        {
            Content = dto.Content,
            TaskItemId = dto.TaskItemId,
            UserId = userId
        };

        await taskDbContext.Comments.AddAsync(comment);
        await taskDbContext.SaveChangesAsync();

        return Ok(new { comment.Id, comment.Content, comment.CreatedAt });
    }

    [HttpGet("task/{taskId}")]
    public async Task<IActionResult> GetComments(Guid taskId)
    {
        var comments = await taskDbContext.Comments
            .Where(c => c.TaskItemId == taskId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return Ok(comments);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateComment(UpdateCommentDto dto)
    {
        var comment = await taskDbContext.Comments.FindAsync(dto.Id);
        if (comment == null) return NotFound("Comment not found");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (comment.UserId != userId) return Forbid();

        comment.Content = dto.Content;
        await taskDbContext.SaveChangesAsync();

        return Ok(new { comment.Id, Message = "Comment updated" });
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteComment(Guid id)
    {
        var comment = await taskDbContext.Comments.FindAsync(id);
        if (comment == null) return NotFound("Comment not found");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (comment.UserId != userId) return Forbid();

        taskDbContext.Comments.Remove(comment);
        await taskDbContext.SaveChangesAsync();

        return NoContent();
    }
}
