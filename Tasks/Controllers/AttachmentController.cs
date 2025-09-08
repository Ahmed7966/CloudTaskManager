using CloudTaskManager.Data;
using CloudTaskManager.DTO_s;
using CloudTaskManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudTaskManager;

[Authorize]
[ApiController]
[Route("api/attachments")]
public class AttachmentController(TaskDbContext taskDbContext) : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> CreateAttachment(CreateAttachmentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var task = await taskDbContext.TaskItems.FindAsync(dto.TaskItemId);
        if (task == null)
            return NotFound("Task not found");

        var attachment = new Attachment
        {
            FileUrl = dto.FileUrl,
            FileName = dto.FileName,
            TaskItemId = dto.TaskItemId
        };

        await taskDbContext.Attachments.AddAsync(attachment);
        await taskDbContext.SaveChangesAsync();

        return Ok(new { attachment.Id, attachment.FileUrl, attachment.FileName, attachment.UploadedAt });
    }

    [HttpGet("{taskId:guid}")]
    public async Task<IActionResult> GetAttachments(Guid taskId)
    {
        var attachments = await taskDbContext.Attachments
            .Where(a => a.TaskItemId == taskId)
            .ToListAsync();

        return Ok(attachments);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateAttachment(UpdateAttachmentDto dto)
    {
        var attachment = await taskDbContext.Attachments.FindAsync(dto.Id);
        if (attachment == null)
            return NotFound("Attachment not found");

        if (!string.IsNullOrEmpty(dto.FileUrl))
            attachment.FileUrl = dto.FileUrl;

        if (!string.IsNullOrEmpty(dto.FileName))
            attachment.FileName = dto.FileName;

        await taskDbContext.SaveChangesAsync();

        return Ok(new { attachment.Id, Message = "Attachment updated successfully" });
    }

    [HttpDelete("delete/{id:int}")]
    public async Task<IActionResult> DeleteAttachment(int id)
    {
        var attachment = await taskDbContext.Attachments.FindAsync(id);
        if (attachment == null)
            return NotFound("Attachment not found");

        taskDbContext.Attachments.Remove(attachment);
        await taskDbContext.SaveChangesAsync();

        return NoContent();
    }
}
