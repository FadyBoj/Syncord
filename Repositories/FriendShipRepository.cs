using Microsoft.EntityFrameworkCore;
using Syncord.Data;
using Syncord.Models;

namespace Syncord.Repositories;

public class OperationResult
{
    public bool Succeeded { get; set; }
    public string? ErrorMessage { get; set; }
}

public interface IFriendShipRepository
{
    Task<OperationResult> SendFriendRequest(string senderId, string recieverId);
}

public class FriendShipRepository : IFriendShipRepository
{
    private readonly SyncordContext _context;

    public FriendShipRepository(SyncordContext context)
    {
        _context = context;
    }

    public async Task<OperationResult> SendFriendRequest(string senderId, string recieverId)
    {
        var orderResult = StringComparer.OrdinalIgnoreCase.Compare(senderId, recieverId);
        var combinedIds = (orderResult < 0 ? senderId : recieverId).ToString() +
        (orderResult < 0 ? recieverId : senderId).ToString();

        var existingFriendRequest = await _context.friendRequests.FirstOrDefaultAsync(
            f => f.CombinedIds == combinedIds
        );


        if (existingFriendRequest != null)
            return new OperationResult
            {
                Succeeded = false,
                ErrorMessage = "Friend request already sent"
            };


        var sender = await _context.Users.FirstOrDefaultAsync(u => u.Id == senderId);
        var reciever = await _context.Users.FirstOrDefaultAsync(u => u.Id == recieverId);


        if (sender == null)
            return new OperationResult
            {
                Succeeded = false,
                ErrorMessage = "Invalid senderID"
            };

        if (reciever == null)
            return new OperationResult
            {
                Succeeded = false,
                ErrorMessage = "Can't find the user you're looking for"
            };

        var newFriendRequest = new FriendRequest
        {
            SenderId = sender.Id,
            RecieverId = reciever.Id,
            CombinedIds = combinedIds
        };

        await _context.friendRequests.AddAsync(newFriendRequest);
        await _context.SaveChangesAsync();

        return new OperationResult
        {
            Succeeded = true,
            ErrorMessage = null
        };
    }

}