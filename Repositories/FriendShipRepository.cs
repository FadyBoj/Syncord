using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using Syncord.Data;
using Syncord.Models;
using Syncord.ViewModels;

namespace Syncord.Repositories;

public class OperationResult
{
    public bool Succeeded { get; set; }
    public string? ErrorMessage { get; set; }

    public string? recieverId { get; set; }
    public string? SenderId { get; set; }
    public string? MessageId { get; set; }
    public SearchFriendVm? User { get; set; }
    public SearchFriendVm? Reciever { get; set; }
    public SearchFriendVm? Sender { get; set; }
    public GetRequestVm? Request { get; set; }
    public GetRequestVm? ReceiverRequest { get; set; }
    public string? RequestId { get; set; }
    public string? UserId { get; set; }
}



public interface IFriendShipRepository
{
    Task<OperationResult> SendFriendRequest(string senderId, string recieverEmail);
    Task<OperationResult> AcceptFriendReuqest(int requestId, string userId);
    Task<OperationResult> RejectFriendReuqest(int requestId, string userId);
    Task<IEnumerable<FriendVm>> GetFriends(string userId);
    Task<List<string>> GetFriendsIds(string userId);
    public Task<List<SearchFriendVm>> Search(string searchString, string userId);
}

public class FriendShipRepository : IFriendShipRepository
{
    private readonly SyncordContext _context;

    public FriendShipRepository(SyncordContext context)
    {
        _context = context;
    }

    public async Task<OperationResult> SendFriendRequest(string senderId, string recieverEmail)
    {
        var recieverUser = await _context.Users.
        Include(u => u.FriendShips)
        .ThenInclude(fs => fs.User1)
        .Include(u => u.FriendShips)
         .ThenInclude(fs => fs.User2)
         .Include(u => u.FriendShipsHolder)
        .ThenInclude(fs => fs.User1)
        .Include(u => u.FriendShipsHolder)
         .ThenInclude(fs => fs.User2)
        .FirstOrDefaultAsync(u => u.Email.ToLower() == recieverEmail.ToLower());

        if (recieverUser == null)
            return new OperationResult
            {
                Succeeded = false,
                ErrorMessage = "User doesn't exist"
            };

        var recieverId = recieverUser.Id.ToString();

        List<string> friendsIds1 = new List<string>();
        List<string> friendsIds2 = new List<string>();

        foreach (var item in recieverUser.FriendShips)
        {
            friendsIds1.Add(item.UserId1 != recieverId ? item.UserId1 : item.UserId2);
        }

        foreach (var item in recieverUser.FriendShipsHolder)
        {
            friendsIds2.Add(item.UserId1 != recieverId ? item.UserId1 : item.UserId2);
        }

        if (friendsIds1.Contains(senderId) || friendsIds2.Contains(senderId))
            return new OperationResult
            {
                Succeeded = false,
                ErrorMessage = "You're already friends with this user"
            };




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
            CombinedIds = combinedIds,
            CreatedAt = DateTime.UtcNow
        };

        await _context.friendRequests.AddAsync(newFriendRequest);
        await _context.SaveChangesAsync();

        return new OperationResult
        {
            Succeeded = true,
            ErrorMessage = null,
            recieverId = recieverId,
            Request = new GetRequestVm
            {
                Id = newFriendRequest.Id,
                UserId = sender.Id,
                OutGoing = false,
                Image = sender.Image,
                Email = sender.Email,
                Firstname = sender.Firstname,
                Lastname = sender.Lastname,
                CreatedAt = DateTime.UtcNow
            },
            ReceiverRequest = new GetRequestVm
            {
                Id = newFriendRequest.Id,
                UserId = reciever.Id,
                OutGoing = true,
                Image = reciever.Image,
                Email = reciever.Email,
                Firstname = reciever.Firstname,
                Lastname = reciever.Lastname,
                CreatedAt = DateTime.UtcNow
            }
        };
    }

    public async Task<OperationResult> AcceptFriendReuqest(int requestId, string userId)
    {
        var existingFriendRequest = await _context.friendRequests
        .Include(fr => fr.Reciever)
        .Include(fr => fr.Sender)
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
            ErrorMessage = "Friend request accepted successfully",
            Reciever = new SearchFriendVm
            {
                Id = existingFriendRequest.Reciever.Id,
                Email = existingFriendRequest.Reciever.Email,
                Firstname = existingFriendRequest.Reciever.Firstname,
                Lastname = existingFriendRequest.Reciever.Lastname,
                Image = existingFriendRequest.Reciever.Image,
                FriendShipId = newFriendShip.Id.ToString(),
                IsOnline = existingFriendRequest.Reciever.IsOnline,
                RequestId = existingFriendRequest.Id.ToString()

            },
            Sender = new SearchFriendVm
            {
                Id = existingFriendRequest.Sender.Id,
                Email = existingFriendRequest.Sender.Email,
                Firstname = existingFriendRequest.Sender.Firstname,
                Lastname = existingFriendRequest.Sender.Lastname,
                Image = existingFriendRequest.Sender.Image,
                FriendShipId = newFriendShip.Id.ToString(),
                IsOnline = existingFriendRequest.Sender.IsOnline,
                RequestId = existingFriendRequest.Id.ToString()
            },
            SenderId = senderId,
        };
    }

    public async Task<OperationResult> RejectFriendReuqest(int requestId, string userId)
    {
        var existingFriendRequest = await _context.friendRequests
                .FirstOrDefaultAsync(
                    fr => fr.Id == requestId && (fr.RecieverId == userId || fr.SenderId == userId)
                    );

        if (existingFriendRequest == null)
            return new OperationResult
            {
                Succeeded = false,
                ErrorMessage = "Friend reuqest doesn't exist"
            };

        _context.friendRequests.Remove(existingFriendRequest);
        await _context.SaveChangesAsync();

        var UserId = existingFriendRequest.SenderId != userId ? existingFriendRequest.SenderId : existingFriendRequest.RecieverId;

        return new OperationResult
        {
            Succeeded = true,
            ErrorMessage = null,
            RequestId = existingFriendRequest.Id.ToString(),
            UserId = UserId
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

    public async Task<List<SearchFriendVm>> Search(string searchString, string userId)
    {
        var users = await _context.Users.Where(
        u => u.Email.ToLower().StartsWith(searchString.ToLower()) && u.Id != userId
            ).Select(u => new SearchFriendVm
            {
                Id = u.Id,
                Firstname = u.Firstname,
                Lastname = u.Lastname,
                Email = u.Email,
                Image = u.Image
            }).ToListAsync();

        return users;

    }

}