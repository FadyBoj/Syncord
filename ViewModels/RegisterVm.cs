using System.ComponentModel.DataAnnotations;

namespace Syncord.ViewModels;

public class RegisterVm
{
    [Required, EmailAddress]
    public string? Email { get; set; }

    [Required]
    public string? Firstname { get; set; }

    [Required]
    public string? Lastname { get; set; }

    [Required]

    public string? Password { get; set; }
}