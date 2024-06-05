using Microsoft.EntityFrameworkCore;
using Syncord.Data;
using Syncord.Models;
using Syncord.ViewModels;

namespace Syncord.Repositories;

public class OperationResult
{
    public bool Succeeded { get; set; }
    public string? ErrorMessage { get; set; }

    public string? recieverId { get; set; }

    public string? MessageId {get;set;}
}

public class FriendId
{
    public string Id { get; set; }
}

public interface IFriendShipRepository
{
    Task<OperationResult> SendFriendRequest(string senderId, string recieverId);
    Task<OperationResult> AcceptFriendReuqest(int requestId, string userId);
    Task<OperationResult> RejectFriendReuqest(int requestId, string userId);
    Task<IEnumerable<FriendVm>> GetFriends(string userId);
    Task<List<string>> GetFriendsIds(string userId);
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

    public async Task<OperationResult> AcceptFriendReuqest(int requestId, string userId)
    {
        var existingFriendRequest = await _context.friendRequests
        .FirstOrDefaultAsync(fr => fr.Id == requestId && fr.RecieverId == userId);

        if (existingFriendRequest == null)
            return new OperationResult
            {
                Succeeded = false,
                ErrorMessage = "Friend reuqest doesn't exist"
            };

        var senderId = existingFriendRequest.SenderId;
        var recieverId = existingFriendRequest.RecieverId;
        var sortResult = StringComparer.OrdinalIgnoreCase.Compare(senderId, recieverId);

        var newFriendShip = new FriendShip
        {
            UserId1 = sortResult < 0 ? senderId : recieverId,
            UserId2 = sortResult < 0 ? recieverId : senderId
        };

        await _context.FriendShips.AddAsync(newFriendShip);
        _context.friendRequests.Remove(existingFriendRequest);

        await _context.SaveChangesAsync();

        return new OperationResult
        {
            Succeeded = true,
            ErrorMessage = "Friend request accepted successfully"
        };
    }

    public async Task<OperationResult> RejectFriendReuqest(int requestId, string userId)
    {
        var existingFriendRequest = await _context.friendRequests
                .FirstOrDefaultAsync(fr => fr.Id == requestId && fr.RecieverId == userId);

        if (existingFriendRequest == null)
            return new OperationResult
            {
                Succeeded = false,
                ErrorMessage = "Friend reuqest doesn't exist"
            };

        _context.friendRequests.Remove(existingFriendRequest);
        await _context.SaveChangesAsync();

        return new OperationResult
        {
            Succeeded = true,
            ErrorMessage = null
        };
    }

    public async Task<IEnumerable<FriendVm>> GetFriends(string userId)
    {
        var friendShips = await _context.FriendShips
        .Include(fs => fs.User1)
        .Include(fs => fs.User2)
        .Where(fs => fs.UserId1 == userId || fs.UserId2 == userId)
        .ToListAsync();


        var friends = friendShips.Select(fs => new FriendVm
        {
            FriendShipId = fs.Id.ToString(),
            UserId = fs.User1.Id != userId ? fs.User1.Id : fs.User2.Id,
            Email = fs.User1.Id != userId ? fs.User1.Email : fs.User2.Email,
            Firstname = fs.User1.Id != userId ? fs.User1.Firstname : fs.User2.Firstname,
            Lastname = fs.User1.Id != userId ? fs.User1.Lastname : fs.User2.Lastname,

        }).ToList();

        return friends;

    }

    public async Task<List<string>> GetFriendsIds(string userId)
    {
        var friendShips = await _context.FriendShips
        .Include(fs => fs.User1)
        .Include(fs => fs.User2)
        .Where(fs => fs.UserId1 == userId || fs.UserId2 == userId)
        .ToListAsync();


        var friends = friendShips.Select(fs =>

             fs.User1.Id != userId ? fs.User1.Id : fs.User2.Id

        ).ToList();

        return friends;

    }

}