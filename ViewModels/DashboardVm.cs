namespace Syncord.ViewModels;

public class DashboardVm
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }

    public string? Image {get;set;}
    public ICollection<GetRequestVm> Requests { get; set; }

    public ICollection<FriendVm> Friends { get; set; }

}