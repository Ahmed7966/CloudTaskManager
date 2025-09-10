namespace CloudTaskManager.Blazor.Services;
using Microsoft.AspNetCore.SignalR.Client;

public class NotificationHubClient : IAsyncDisposable
{
    private readonly HubConnection _connection;

    public NotificationHubClient()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl($"http://localhost:5114/hub/notifications")
            .WithAutomaticReconnect()
            .Build();
    }

    public event Action<string, object>? OnEventReceived;

    public async Task StartAsync()
    {
        _connection.On<object>("taskCreated", payload =>
            OnEventReceived?.Invoke("task.created", payload));

        _connection.On<object>("taskUpdated", payload =>
            OnEventReceived?.Invoke("task.updated", payload));

        _connection.On<object>("reminderDue", payload =>
            OnEventReceived?.Invoke("reminder.due", payload));

        await _connection.StartAsync();
    }

    public Task JoinBoardAsync(int boardId) =>
        _connection.InvokeAsync("JoinBoard", boardId);

    public Task LeaveBoardAsync(int boardId) =>
        _connection.InvokeAsync("LeaveBoard", boardId);

    public Task JoinUserAsync(string userId) =>
        _connection.InvokeAsync("JoinUser", userId);

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}

