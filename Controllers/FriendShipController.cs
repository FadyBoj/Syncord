using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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

        public FriendShipController(IFriendShipRepository friendShipRepository)
        {
            _friendShipRepository = friendShipRepository;
        }

        [HttpPost]
        [Route("Send-request")]
        [Authorize]
        public async Task<ActionResult> SendFriendRequest(FriendRequestVm data)
        {
            var userId = HttpContext.User.FindFirst("Id")?.Value;
            var userEmail = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            if (data.recieverEmail.ToLower() == userEmail.ToLower())
                return BadRequest(new
                {
                    msg = "Can't send friend request to yourself",
                    statusCode = 400
                });

            var result = await _friendShipRepository.SendFriendRequest(userId, data.recieverEmail);
            if (!result.Succeeded)
                return BadRequest(result.ErrorMessage);

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