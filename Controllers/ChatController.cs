using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Syncord.Hubs;
using Syncord.Repositories;
using Syncord.ViewModels;

namespace Syncord.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : Controller
    {

        private readonly IChatRepository _chatRepository;
        private readonly IHubContext<MsgHub> _hubContext;
        public ChatController(IChatRepository chatRepository, IHubContext<MsgHub> hubContext)
        {
            _chatRepository = chatRepository;
            _hubContext = hubContext;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> SendMessage(SendMessageVm data)
        {
            var userId = HttpContext.User.FindFirst("Id")?.Value;

           var result =  await _chatRepository.SendMessage(data.FriendShipId,data.Message,userId);

            if(!result.Succeeded)
            return BadRequest(result.ErrorMessage);
            
            var recieverId = result.recieverId;
            await _hubContext.Clients.User(recieverId).SendAsync("  ",data.Message);

            return Ok("Message sent");
        }
    }
}