using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Syncord.Data;
using Syncord.Hubs;
using Syncord.Models;
using Syncord.ViewModels;

namespace Syncord.Repositories;

public interface IChatRepository
{
    Task<OperationResult> SendMessage(int FriendShipId, string message, string userId);
    Task<IEnumerable<GetMessageVm>> GetMessages(int FriendShipId, string userId, int skip);

}

public class ChatRepository : IChatRepository
{
    private readonly SyncordContext _context;
    private readonly IHubContext<MsgHub> _hubContext;

    public ChatRepository(SyncordContext context, IHubContext<MsgHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task<OperationResult>? SendMessage(int FriendShipId, string message, string userId)
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

        var recieverId = friendShip.UserId1 != userId ? friendShip.UserId1 : friendShip.UserId2;

        var newMessage = new Message
        {
            message = message,
            SenderId = userId,
            FriendShipId = friendShip.Id,
            CreatedAt =  DateTime.UtcNow
        };
        Console.WriteLine(newMessage.CreatedAt);

        await _context.Messages.AddAsync(newMessage);
        await _context.SaveChangesAsync();

        return new OperationResult
        {
            Succeeded = true,
            ErrorMessage = null,
            recieverId = recieverId
        };


    }

    public async Task<IEnumerable<GetMessageVm>> GetMessages(int FriendShipId, string userId, int skip)
    {
        var friendShip = await _context.FriendShips
        .Include(fs => fs.Messages)
        .FirstOrDefaultAsync(fs => fs.Id == FriendShipId);

        if (
        friendShip == null ||
        (friendShip.UserId1 != userId && friendShip.UserId2 != userId)
         )
        {
            return new List<GetMessageVm>();
        }

        var messages = friendShip.Messages
        .OrderByDescending(m => m.CreatedAt)
        .Skip(skip)
        .Take(20)
        .Reverse()
        .Select(m => new GetMessageVm
        {
            Id = m.Id,
            Text = m.message,
            SenderId = m.SenderId,
            IsSent = m.SenderId == userId ? true : false,
            CreatedAt = m.CreatedAt
        }).ToList();

        return messages;

    }

}