using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Syncord.Data;
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
        private readonly SyncordContext _context;

        public FriendShipController(IFriendShipRepository friendShipRepository, IHubContext<MsgHub> hubContext,
        SyncordContext context
        )
        {
            _friendShipRepository = friendShipRepository;
            _hubContext = hubContext;
            _context = context;
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
            if (!result.Succeeded || result.recieverId == null)
                return BadRequest(result.ErrorMessage);

            await _hubContext.Clients.User(result.recieverId).SendAsync("SentRequest", result.Request);

            return Ok(result.ReceiverRequest);
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

            //Sending a singnal to the sender that the request has been accepted
            await _hubContext.Clients.User(result.SenderId).SendAsync("RequestAccepted", result.Reciever);

            return Ok(result.Sender);

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

            await _hubContext.Clients.User(result.UserId).SendAsync("RequestRejected", result.RequestId);

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

        [HttpDelete]
        [Authorize]
        [Route("delete-friend")]
        public async Task<ActionResult<bool>> DeleteFriend(DeleteFriendVm data)
        {
            var userId = HttpContext.User.FindFirst("Id")?.Value;
            var friendShip = await _context.FriendShips.FirstOrDefaultAsync(fs => fs.Id.ToString() == data.FriendShipId);
            if (friendShip == null)
                return BadRequest(
                    new
                    {
                        msg = "Friend ship doens't exist",
                        statusCode = 400
                    }
                );
            if (friendShip.UserId1 != userId && friendShip.UserId2 != userId)
                return StatusCode(403,
                    new
                    {
                        msg = "Can't delete friendship",
                        statusCode = 403
                    });

            try
            {
                var receiverUserId = friendShip.UserId1 != userId ? friendShip.UserId1 : friendShip.UserId2;
                await _friendShipRepository.DeleteFriend(friendShip);
                await _hubContext.Clients.User(receiverUserId).SendAsync("friendshipDeleted", friendShip.Id);
            }
            catch (Exception err)
            {
                return StatusCode(500, new
                {
                    msg = "Something went wrong",
                    statusCode = 500
                });
            }


            return Ok(
                    new
                    {
                        msg = "Friendship deleted successfully",
                        statusCode = 200
                    }
            );
        }

    }
}