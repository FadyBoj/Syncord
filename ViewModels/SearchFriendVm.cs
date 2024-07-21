namespace Syncord.ViewModels;

public class SearchFriendVm
{
    public string Id { get; set; }

    public string Firstname { get; set; }

    public string Lastname { get; set; }

    public string Email { get; set; }

    public string Image { get; set; }

    public bool? IsOnline { get; set; }

    public string? FriendShipId { get; set; }

    public string? RequestId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool isSent {get;set;}
}