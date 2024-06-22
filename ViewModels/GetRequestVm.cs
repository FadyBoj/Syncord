namespace Syncord.ViewModels;

public class GetRequestVm
{
    public long Id { get; set; }
    public string UserId { get; set; }
    public bool OutGoing { get; set; }
    public string Image { get; set; }
    public string Email { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public DateTime CreatedAt { get; set; }


}