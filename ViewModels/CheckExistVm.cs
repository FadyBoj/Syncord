using System.ComponentModel.DataAnnotations;

namespace Syncord.ViewModels;

public class CheckExistVm
{
    [Required]
    public string Email {get;set;} 
}