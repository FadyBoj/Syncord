using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Syncord.Data;
using Syncord.Models;
using Syncord.ViewModels;

namespace Syncord.Repositories;

public interface IUserRepository
{
    Task<IdentityResult> AddUser(RegisterVm user);
    Task<User?> GetUserByEmail(string Email);
    Task AddOnline(string id);
    Task RemoveOnline(string id);
    Task<ICollection<GetRequestVm>> GetRequests(string id);
    Task<bool> IsUserExist(string email);
    Task<Object> Dashboard(string id);
    Task<OperationResult> UploadProfilePicture(string userId, string image);

}



public class UserRepository : IUserRepository
{
    private readonly SyncordContext _context;
    private readonly UserManager<User> _userManager;

    public UserRepository(SyncordContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<User?> GetUserByEmail(string Email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == Email);
        return user;
    }

    public async Task<IdentityResult> AddUser(RegisterVm user)
    {
        var newUser = new User
        {
            Email = user.Email,
            Firstname = user.Firstname,
            Lastname = user.Lastname,
            UserName = user.Email
        };

        var result = await _userManager.CreateAsync(newUser, user.Password);

        return result;
    }

    public async Task AddOnline(string id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return;

        user.IsOnline = true;
        await _context.SaveChangesAsync();
    }

    public async Task RemoveOnline(string id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return;

        user.IsOnline = false;
        await _context.SaveChangesAsync();
    }


    public async Task<ICollection<GetRequestVm>> GetRequests(string id)
    {
        var friednRequests = _context.friendRequests
        .Include(fr => fr.Reciever)
        .Include(fr => fr.Sender);

        var sent = friednRequests.Where(fr => fr.Sender.Id == id).Select(fr => new GetRequestVm
        {
            Id = fr.Id,
            Email = fr.Reciever.Email,
            OutGoing = true
        }).ToList();

        var recieved = friednRequests.Where(fr => fr.Reciever.Id == id).Select(fr => new GetRequestVm
        {
            Id = fr.Id,
            Email = fr.Sender.Email,
            OutGoing = false
        }).ToList();

        var allRequests = sent.Concat(recieved).ToList();

        return allRequests;
    }

    public async Task<bool> IsUserExist(string email)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        if (existingUser == null)
            return false;

        return true;
    }

    public async Task<Object> Dashboard(string id)
    {
        var user = await _context.Users.Where(u => u.Id == id)
        .Include(u => u.FriendShips)
             .ThenInclude(fs => fs.User1)
        .Include(u => u.FriendShips)
            .ThenInclude(fs => fs.User2)
        .Include(u => u.FriendShipsHolder)
             .ThenInclude(fs => fs.User1)
        .Include(fs => fs.FriendShipsHolder)
             .ThenInclude(fs => fs.User2)
        .Include(u => u.SentFriendRequests)
            .ThenInclude(sf => sf.Reciever)
        .Include(u => u.RecievedFriendRequests)
            .ThenInclude(rf => rf.Sender)
        .FirstOrDefaultAsync();

        //Formating friends
        var friends = user.FriendShips.Select(fs => new FriendVm
        {
            Id = fs.UserId1 != id ? fs.UserId1 : fs.UserId2,
            Email = fs.UserId1 != id ? fs.User1.Email : fs.User2.Email,
            Firstname = fs.UserId1 != id ? fs.User1.Firstname : fs.User2.Firstname,
            Lastname = fs.UserId1 != id ? fs.User1.Lastname : fs.User2.Lastname,
            Image = fs.UserId1 != id ? fs.User1.Image : fs.User2.Image,
            IsOnline = fs.UserId1 != id ? fs.User1.IsOnline : fs.User2.IsOnline,
        }).ToList();

        var friendsHolder = user.FriendShipsHolder.Select(fs => new FriendVm
        {
            Id = fs.UserId1 != id ? fs.UserId1 : fs.UserId2,
            Email = fs.UserId1 != id ? fs.User1.Email : fs.User2.Email,
            Firstname = fs.UserId1 != id ? fs.User1.Firstname : fs.User2.Firstname,
            Lastname = fs.UserId1 != id ? fs.User1.Lastname : fs.User2.Lastname,
            Image = fs.UserId1 != id ? fs.User1.Image : fs.User2.Image,
            IsOnline = fs.UserId1 != id ? fs.User1.IsOnline : fs.User2.IsOnline,
        }).ToList();

        //Formating requests

        var recievedRequests = user.RecievedFriendRequests.Select(rf => new GetRequestVm
        {
            Id = rf.Id,
            UserId = rf.SenderId,
            Email = rf.Sender.Email,
            OutGoing = false
        });

        var sentRequests = user.SentFriendRequests.Select(rf => new GetRequestVm
        {
            Id = rf.Id,
            UserId = rf.RecieverId,
            Email = rf.Reciever.Email,
            OutGoing = true
        });

        var dashboard = new DashboardVm
        {
            Id = user.Id,
            Email = user.Email,
            Firstname = user.Firstname,
            Lastname = user.Lastname,
            Image = user.Image,
            Requests = recievedRequests.Concat(sentRequests).ToList(),
            Friends = friends.Concat(friendsHolder).ToList()
        };

        return dashboard;
    }

    public async Task<OperationResult> UploadProfilePicture(string userId, string image)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return new OperationResult
            {
                Succeeded = false,
                ErrorMessage = "User is not found"
            };

        user.Image = image;
        _context.SaveChangesAsync();

        return new OperationResult
        {
            Succeeded = true,
            ErrorMessage = null
        };

    }


}