using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Syncord.Models;


[Index(nameof(Email), IsUnique = true)]
public class User : IdentityUser
{
    [Required]
    public override string? Email { get; set; }

    [Required]
    public string? Firstname { get; set; }

    [Required]
    public string? Lastname { get; set; }

    [Required, MinLength(8)]
    public override string? PasswordHash { get; set; }

    public bool IsOnline { get; set; } = false;

    public ICollection<FriendRequest> SentFriendRequests { get; } = new List<FriendRequest>();
    public ICollection<FriendRequest> RecievedFriendRequests { get; } = new List<FriendRequest>();

}