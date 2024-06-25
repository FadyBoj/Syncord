using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Syncord.Hubs;
using Syncord.Models;
using Syncord.Repositories;
using Syncord.ViewModels;

namespace Syncord.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FriendShipController : Controller
    {
        private readonly IFriendShipRepository _friendShipRepository;
        private readonly IHubContext<MsgHub> _hubContext;

        public FriendShipController(IFriendShipRepository friendShipRepository, IHubContext<MsgHub> hubContext)
        {
            _friendShipRepository = friendShipRepository;
            _hubContext = hubContext;
        }

        [HttpPost]
        [Route("Send-request")]
        [Authorize]
        public async Task<ActionResult> SendFriendRequest(FriendRequestVm data)
        {
            var userId = HttpContext.User.FindFirst("Id")?.Value;
            var userEmail = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            if (data.recieverEmail.ToLower() == userEmail.ToLower())
                return BadRequest("Can't send friend request to yourself");

            var result = await _friendShipRepository.SendFriendRequest(userId, data.recieverEmail);
            if (!result.Succeeded)
                return BadRequest(result.ErrorMessage);

            await _hubContext.Clients.User(result.recieverId).SendAsync("SentRequest", result.User);

            return Ok("Success");
        }

        [HttpPost]
        [Route("accept-request")]
        [Authorize]
        public async Task<ActionResult> AcceptFriendRequest(AcceptsRequestVMm data)
        {
            var userId = HttpContext.User.FindFirst("Id")?.Value;
            var result = await _friendShipRepository.AcceptFriendReuqest(data.RequestId, userId);

            if (!result.Succeeded)
                return BadRequest(result.ErrorMessage);

            Console.WriteLine(result.User.Email);
            Console.WriteLine(result.User.Id);

            //Sending a singnal to the sender that the request has been accepted
            await _hubContext.Clients.User(result.SenderId).SendAsync("RequestAccepted", result.User);

            return Ok("Friend request accepted successfully");

        }

        [HttpPost]
        [Route("reject-request")]
        [Authorize]
        public async Task<ActionResult> RejectFriendRequest(AcceptsRequestVMm data)
        {

            var userId = HttpContext.User.FindFirst("Id")?.Value;
            var result = await _friendShipRepository.RejectFriendReuqest(data.RequestId, userId);

            if (!result.Succeeded)
                return BadRequest(result.ErrorMessage);

            return Ok("Friend request rejected successfully");

        }

        [HttpGet]
        [Authorize]
        [Route("Search")]

        public async Task<ActionResult<SearchFriendVm>> Search(string search)
        {
            var userId = HttpContext.User.FindFirst("Id")?.Value;

            var users = await _friendShipRepository.Search(search, userId);
            return Ok(users);
        }

    }
}