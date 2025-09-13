using CloudTaskManager.Data;
using CloudTaskManager.Message;
using Microsoft.EntityFrameworkCore;

namespace CloudTaskManager.Background;

public class ReminderDueWorker(IServiceProvider serviceProvider, ILogger<ReminderDueWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
                var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

                var now = DateTime.UtcNow;

                var dueReminders = await db.Reminders
                    .Include(r => r.TaskItem)
                    .Where(r => r.ReminderTime <= now)
                    .ToListAsync(stoppingToken);

                foreach (var reminder in dueReminders)
                {
                    var userId = reminder.TaskItem?.AssignedToUserId;

                    await publisher.PublishReminderDue(reminder, userId);

                    reminder.IsSent = true;
                }

                await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing reminders");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // check every minute
        }
    }
}
