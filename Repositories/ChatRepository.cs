using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Syncord.Data;
using Syncord.Hubs;
using Syncord.Models;

namespace Syncord.Repositories;

public interface IChatRepository
{
    Task<OperationResult> SendMessage(int FriendShipId, string message, string userId);
}

public class ChatRepository : IChatRepository
{
    private readonly SyncordContext _context;
    private readonly IHubContext<MsgHub> _hubContext;

    public ChatRepository(SyncordContext context,IHubContext<MsgHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task<OperationResult> SendMessage(int FriendShipId, string message, string userId)
    {
        var friendShip = await _context.FriendShips.FirstOrDefaultAsync(fs => fs.Id == FriendShipId);
        var recieverId = friendShip.UserId1 != userId ? friendShip.UserId1 : friendShip.UserId2;

        if (
            friendShip == null ||
            (friendShip.UserId1 != userId && friendShip.UserId2 != userId)
        )
            return new OperationResult
            {
                Succeeded = false,
                ErrorMessage = "Forbidden"
            };

        var newMessage = new Message
        {
            message = message,
            SenderId = userId,
            FriendShipId = friendShip.Id
        };

        await _context.Messages.AddAsync(newMessage);
        await _context.SaveChangesAsync();

        return new OperationResult
        {
            Succeeded = true,
            ErrorMessage = null,
            recieverId = recieverId
        };


    }

}