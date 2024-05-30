using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Syncord.Repositories;

namespace Syncord.Hubs
{
    [Authorize]
    public class MsgHub : Hub
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public MsgHub(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }


        public override async Task<System.Threading.Tasks.Task> OnConnectedAsync()
        {
            var userId = Context.User.FindFirst("Id")?.Value;

            if (userId == null)
                return base.OnConnectedAsync();


            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var _userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                var _friendShipRepository = scope.ServiceProvider.GetRequiredService<IFriendShipRepository>();

                await _userRepository.AddOnline(userId);

                //Send signals to all the user friends 
                var friendsIds = await _friendShipRepository.GetFriendsIds(userId);
                await Clients.Users(friendsIds).SendAsync("hoppedOnline", userId);
            }

            return base.OnConnectedAsync();
        }

        public async override Task<System.Threading.Tasks.Task> OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User.FindFirst("Id")?.Value;

            if (userId == null)
                return base.OnDisconnectedAsync(exception);

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var _userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                var _friendShipRepository = scope.ServiceProvider.GetRequiredService<IFriendShipRepository>();

                await _userRepository.RemoveOnline(userId);

                //Send signals to all the user friends 
                var friendsIds = await _friendShipRepository.GetFriendsIds(userId);
                await Clients.Users(friendsIds).SendAsync("wentOffline", userId);
            }

            return base.OnDisconnectedAsync(exception);
        }
    }
}

