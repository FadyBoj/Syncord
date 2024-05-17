using System.ComponentModel.DataAnnotations;

namespace Syncord.ViewModels;

public class SendMessageVm
{
    [Required]
    public int FriendShipId {get;set;}

    [Required]
    public string Message {get;set;}
}