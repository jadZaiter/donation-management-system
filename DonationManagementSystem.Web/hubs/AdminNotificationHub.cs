using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DonationManagementSystem.Web.Hubs
{
    [Authorize] // must be logged in
    public class AdminNotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            // Join Admins group if user has Admin role
            if (Context.User?.IsInRole("Admin") == true)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            }

            await base.OnConnectedAsync();
        }
    }
}
