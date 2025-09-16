using CloudTaskManager.Shared.Correlation;

namespace CloudTaskManager.Notifications.Hub;

using Microsoft.AspNetCore.SignalR;

public class NotificationHub(ILogger<NotificationHub> logger, ICorrelationIdAccessor correlationIdAccessor) : Hub
{
    public override async Task OnConnectedAsync()
    {
        logger.LogInformation("Client connected: {ConnectionId} [CorrelationId: {CorrelationId}]",
            Context.ConnectionId,
            correlationIdAccessor.CorrelationId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogInformation("Client disconnected: {ConnectionId}, Reason: {Reason} [CorrelationId: {CorrelationId}]",
            Context.ConnectionId,
            exception?.Message ?? "Normal closure",
            correlationIdAccessor.CorrelationId);

        await base.OnDisconnectedAsync(exception);
    }

    public Task JoinBoard(int boardId)
    {
        logger.LogInformation("Client {ConnectionId} joined board:{BoardId} [CorrelationId: {CorrelationId}]",
            Context.ConnectionId,
            boardId,
            correlationIdAccessor.CorrelationId);

        return Groups.AddToGroupAsync(Context.ConnectionId, $"board:{boardId}");
    }

    public Task JoinUser(string userId)
    {
        logger.LogInformation("Client {ConnectionId} joined user:{UserId} group [CorrelationId: {CorrelationId}]",
            Context.ConnectionId,
            userId,
            correlationIdAccessor.CorrelationId);

        return Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
    }
}