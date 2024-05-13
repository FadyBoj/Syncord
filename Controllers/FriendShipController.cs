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

            var result = await _friendShipRepository.SendFriendRequest(userId,data.recieverId);
            if(!result.Succeeded)
            return BadRequest(result.ErrorMessage);

            return Ok("Success");
        }

    }
}