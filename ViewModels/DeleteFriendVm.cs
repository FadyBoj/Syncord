using System.ComponentModel.DataAnnotations;

namespace Syncord.ViewModels;

public class DeleteFriendVm
{
    [Required]
    public string? FriendShipId { get; set; }
}