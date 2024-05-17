using Microsoft.EntityFrameworkCore;
using Syncord.Data;
using Syncord.Models;

namespace Syncord.Repositories;

public interface IChatRepository
{
    Task<OperationResult> SendMessage(int FriendShipId, string message, string userId);
}

public class ChatRepository : IChatRepository
{
    private readonly SyncordContext _context;
    public ChatRepository(SyncordContext context)
    {
        _context = context;
    }

    public async Task<OperationResult> SendMessage(int FriendShipId, string message, string userId)
    {
        var friendShip = await _context.FriendShips.FirstOrDefaultAsync(fs => fs.Id == FriendShipId);

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
            ErrorMessage = null
        };


    }

}