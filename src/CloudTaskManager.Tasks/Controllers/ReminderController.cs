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
[Route("api/reminders")]
public class ReminderController(TaskDbContext taskDbContext,
    ILogger<ReminderController> logger,
    ICorrelationIdAccessor correlationIdAccessor) : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> CreateReminder(CreateReminderDto dto)
    {
        var task = await taskDbContext.TaskItems.FindAsync(dto.TaskItemId);
        if (task == null)
        {
            logger.LogInformation($"Task with id {dto.TaskItemId} was not found [CorrelationId: {correlationIdAccessor.CorrelationId}]");
            return NotFound("Task not found");
        }

        var reminder = new Reminder
        {
            TaskItemId = dto.TaskItemId,
            ReminderTime = dto.ReminderTime
        };

        await taskDbContext.Reminders.AddAsync(reminder);
        await taskDbContext.SaveChangesAsync();

        logger.LogInformation($"Task with id {dto.TaskItemId} created for reminder [CorrelationId: {correlationIdAccessor.CorrelationId}]");
        return Ok(reminder);
    }

    [HttpGet("{taskId}")]
    public async Task<IActionResult> GetReminders(Guid taskId)
    {
        var reminders = await taskDbContext.Reminders
            .Where(r => r.TaskItemId == taskId)
            .ToListAsync();

        return Ok(reminders);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateReminder(UpdateReminderDto dto)
    {
        var reminder = await taskDbContext.Reminders.FindAsync(dto.Id);
        if (reminder == null)
        {
            logger.LogWarning($"Reminder with id {dto.Id} was not found [CorrelationId: {correlationIdAccessor.CorrelationId}]");
            return NotFound("Reminder not found");
        }

        if (dto.ReminderTime.HasValue) reminder.ReminderTime = dto.ReminderTime.Value;
        if (dto.IsSent.HasValue) reminder.IsSent = dto.IsSent.Value;

        await taskDbContext.SaveChangesAsync();
        logger.LogInformation($"Task with id {dto.Id} updated for reminder [CorrelationId: {correlationIdAccessor.CorrelationId}]");
        return Ok(new { reminder.Id, Message = "Reminder updated successfully" });
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteReminder(Guid id)
    {
        var reminder = await taskDbContext.Reminders.FindAsync(id);
        if (reminder == null)
        {
            logger.LogWarning($"Reminder not found for reminder with id {id} [CorrelationId: {correlationIdAccessor.CorrelationId}].");
            return NotFound("Reminder not found");
        }

        taskDbContext.Reminders.Remove(reminder);
        await taskDbContext.SaveChangesAsync();
        
        logger.LogInformation($"Reminder Deleted for reminder with id {id} [CorrelationId: {correlationIdAccessor.CorrelationId}]");
        return NoContent();
    }
}