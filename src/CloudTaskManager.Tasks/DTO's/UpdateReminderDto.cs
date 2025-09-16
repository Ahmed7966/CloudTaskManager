namespace CloudTaskManager.DTO_s;

public record UpdateReminderDto(Guid Id, DateTime? ReminderTime, bool? IsSent);