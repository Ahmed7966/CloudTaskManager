namespace Notifications.Hub;
using Microsoft.AspNetCore.SignalR;

public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public Task JoinBoard(int boardId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, $"board:{boardId}");
    }

    public Task JoinUser(string userId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
    }
}
