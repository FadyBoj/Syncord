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

        if(existingUser == null)
        return false;

        return true;
    }
}