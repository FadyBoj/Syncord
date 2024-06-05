namespace Syncord.ViewModels;

public class AllMessagesVm {
    public string FriendShipId {get;set;}
    public string UserId {get;set;}

    public DateTime LatesMessageDate {get;set;}
    public IEnumerable<GetMessageVm> Messages {get;set;}

}