using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SmartCampus.Web.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var user = Context.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            var userId = Context.UserIdentifier ?? user.Identity.Name;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            }

            // Add user to role groups
            if (user.IsInRole("SuperAdmin")) await Groups.AddToGroupAsync(Context.ConnectionId, "Role_SuperAdmin");
            if (user.IsInRole("CampusAdmin")) await Groups.AddToGroupAsync(Context.ConnectionId, "Role_CampusAdmin");
            if (user.IsInRole("DepartmentSupervisor")) await Groups.AddToGroupAsync(Context.ConnectionId, "Role_DepartmentSupervisor");
            if (user.IsInRole("MaintenanceWorker")) await Groups.AddToGroupAsync(Context.ConnectionId, "Role_MaintenanceWorker");
            if (user.IsInRole("StudentTeacher")) await Groups.AddToGroupAsync(Context.ConnectionId, "Role_StudentTeacher");
        }

        await base.OnConnectedAsync();
    }

    public async Task SendNotification(string targetUserId, string title, string message, string type, string? url)
    {
        await Clients.Group($"User_{targetUserId}").SendAsync("ReceiveNotification", new
        {
            Title = title,
            Message = message,
            Type = type,
            Url = url,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task BroadcastCampusAlert(string title, string message)
    {
        await Clients.All.SendAsync("ReceiveCampusAlert", new
        {
            Title = title,
            Message = message,
            Timestamp = DateTime.UtcNow
        });
    }
}
