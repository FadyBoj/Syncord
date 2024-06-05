using System.ComponentModel.DataAnnotations;

namespace Syncord.Models;

public class FriendShip
{
    [Key]
    public long Id { get; set; }
    public string UserId1 { get; set; }
    public string UserId2 { get; set; }

    public User User1 {get;set;}

    public User User2 {get;set;}

    public DateTime LatestMessageDate {get;set;} = DateTime.UtcNow;

    public ICollection<Message> Messages {get;} = new List<Message>();

}