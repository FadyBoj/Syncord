
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Syncord.Models;

[Index(nameof(CombinedIds),IsUnique = true)]
public class FriendRequest
{
    [Key]
    public long Id { get; set; }

    [Required]
    public string SenderId { get; set; }

    [Required]
    public string RecieverId { get; set; }

    public User Sender { get; set; } = null!;
    public User Reciever { get; set; } = null!;

    public string CombinedIds {get;set;} 

}