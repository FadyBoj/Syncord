using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Syncord.Repositories;
using Syncord.ViewModels;

namespace Syncord.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : Controller
    {

        private readonly IChatRepository _chatRepository;
        public ChatController(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> SendMessage(SendMessageVm data)
        {
            var userId = HttpContext.User.FindFirst("Id")?.Value;

           var result =  await _chatRepository.SendMessage(data.FriendShipId,data.Message,userId);

            if(!result.Succeeded)
            return BadRequest(result.ErrorMessage);

            return Ok("Message sent");
        }
    }
}