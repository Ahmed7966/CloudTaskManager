namespace CloudTaskManager.DTO_s;

public record CreateReminderDto(Guid TaskItemId, DateTime ReminderTime);