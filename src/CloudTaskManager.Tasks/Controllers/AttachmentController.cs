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
[Route("api/attachments")]
public class AttachmentController(
    TaskDbContext taskDbContext,
    ILogger<AttachmentController> logger,
    ICorrelationIdAccessor correlationIdAccessor) : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> CreateAttachment(CreateAttachmentDto dto)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning($"Invalid registration request [CorrelationId: {correlationIdAccessor.CorrelationId}]");
            return BadRequest(ModelState);
        }

        var task = await taskDbContext.TaskItems.FindAsync(dto.TaskItemId);
        if (task == null)
        {
            logger.LogWarning($"Task with id {dto.TaskItemId} not found  [CorrelationId: {correlationIdAccessor.CorrelationId}]");
            return NotFound("Task not found");
        }

        var attachment = new Attachment
        {
            FileUrl = dto.FileUrl,
            FileName = dto.FileName,
            TaskItemId = dto.TaskItemId
        };

        await taskDbContext.Attachments.AddAsync(attachment);
        await taskDbContext.SaveChangesAsync();

        logger.LogInformation($"Created attachment [CorrelationId: {correlationIdAccessor.CorrelationId}]");
        return Ok(new { attachment.Id, attachment.FileUrl, attachment.FileName, attachment.UploadedAt });
    }

    [HttpGet("{taskId:guid}")]
    public async Task<IActionResult> GetAttachments(Guid taskId)
    {
        var attachments = await taskDbContext.Attachments
            .Where(a => a.TaskItemId == taskId)
            .ToListAsync();
        logger.LogInformation($"Attachment not found [CorrelationId: {correlationIdAccessor.CorrelationId}]");
        return Ok(attachments);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateAttachment(UpdateAttachmentDto dto)
    {
        var attachment = await taskDbContext.Attachments.FindAsync(dto.Id);
        if (attachment == null)
        {
            logger.LogWarning($"Attachment not found [CorrelationId: {correlationIdAccessor.CorrelationId}]");
            return NotFound("Attachment not found");
        }

        if (!string.IsNullOrEmpty(dto.FileUrl))
            attachment.FileUrl = dto.FileUrl;

        if (!string.IsNullOrEmpty(dto.FileName))
            attachment.FileName = dto.FileName;

        await taskDbContext.SaveChangesAsync();
        logger.LogInformation($"Updated attachment {attachment.Id} [CorrelationId: {correlationIdAccessor.CorrelationId}]");
        return Ok(new { attachment.Id, Message = "Attachment updated successfully" });
    }

    [HttpDelete("delete/{id:int}")]
    public async Task<IActionResult> DeleteAttachment(int id)
    {
        var attachment = await taskDbContext.Attachments.FindAsync(id);
        if (attachment == null)
        {
            logger.LogWarning($"Attachment not found [CorrelationId: {correlationIdAccessor.CorrelationId}]");
            return NotFound("Attachment not found");
        }

        taskDbContext.Attachments.Remove(attachment);
        await taskDbContext.SaveChangesAsync();
        logger.LogInformation($"Deleted attachment {attachment.Id} [CorrelationId: {correlationIdAccessor.CorrelationId}]");
        return NoContent();
    }
}
