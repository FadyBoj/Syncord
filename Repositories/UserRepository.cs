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
}

public class UserRepository : IUserRepository
{
    private readonly IdentityContext _context;
    private readonly UserManager<User> _userManager;

    public UserRepository(IdentityContext context, UserManager<User> userManager)
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
            PasswordHash = user.Password,
            UserName = user.Email
        };

        var result = await _userManager.CreateAsync(newUser);

        return result;
    }

}