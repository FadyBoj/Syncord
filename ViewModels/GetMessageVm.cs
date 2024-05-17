namespace Syncord.ViewModels;

public class GetMessageVm 
{
    public long Id {get;set;}
    public string Text {get;set;}

    public string SenderId {get;set;}

    public bool IsSent {get;set;}

    public DateTime createdAt {get;set;}
}