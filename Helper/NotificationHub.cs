using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace WangenPizza.Helper
{
    [AllowAnonymous]
    public class NotificationHub:Hub
    {
        public async Task SendNotification(string message , string notificationType)
        {
            await Clients.All.SendAsync("ReceiveNotification", message , notificationType);
        }
    }
}
