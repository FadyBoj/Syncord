using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Syncord.Hubs
{   
    [Authorize]
    public class MsgHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            Console.WriteLine("User connected");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine("User Disconnected");

            return base.OnDisconnectedAsync(exception);
        }
    }
}

