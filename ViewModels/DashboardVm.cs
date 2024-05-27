namespace Syncord.ViewModels;

public class DashboardVm
{

    public string Email { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    
    public ICollection<GetRequestVm> Requests {get;set;}

    public ICollection<FriendVm> Friends {get;set;}

}