
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Syncord.providers;

public class EmailBasedUserIdProvider : IUserIdProvider
{
    public virtual string GetUserId(HubConnectionContext connection)
    {
        Console.WriteLine(connection.User?.FindFirst("Id")?.Value!);
        return connection.User?.FindFirst("Id")?.Value!;
    }
}