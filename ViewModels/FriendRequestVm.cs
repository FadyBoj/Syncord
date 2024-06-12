
using System.ComponentModel.DataAnnotations;

namespace Syncord.ViewModels;

public class FriendRequestVm
{
    [Required]
    public string recieverEmail {get;set;}
}