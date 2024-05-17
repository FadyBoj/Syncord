using System.ComponentModel.DataAnnotations;

namespace Syncord.Models;

public class Message
{
    [Key]
    public long Id { get; set; }

    public string message { get; set; }

    public string SenderId { get; set; }

    public User Sender { get; set; }

    public long FriendShipId { get; set; }

    public FriendShip FriendShip { get; set; } = null!;

    public DateTime CreatedAt {get;set;} =  DateTime.UtcNow;
}