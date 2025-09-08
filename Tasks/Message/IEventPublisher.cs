using CloudTaskManager.Models;

namespace CloudTaskManager.Message;

public interface IEventPublisher
{
    Task PublishTaskCreated(TaskItem task);
    Task PublishTaskUpdated(TaskItem task);
    Task PublishTaskDeleted(Guid taskId);
}
