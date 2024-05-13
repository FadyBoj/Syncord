
using System.ComponentModel.DataAnnotations;

namespace Syncord.ViewModels;

public class FriendRequestVm
{
    [Required]
    public string recieverId {get;set;}
}