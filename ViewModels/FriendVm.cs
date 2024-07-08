
namespace Syncord.ViewModels;

public class FriendVm
{
    public string FriendShipId { get; set; }
    public string UserId { get; set; }
    public string Email { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public string Image { get; set; }
    public bool IsOnline { get; set; }
    public DateTime? CreatedAt { get; set; }
    public GetMessageVm? latestMessage { get; set; }

}