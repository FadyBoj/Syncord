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
            var result = await _chatRepository.SendMessage(data.FriendShipId, data.Message, userId);

            if (!result.Succeeded)
                return BadRequest(result.ErrorMessage);

            var recieverId = result.recieverId;
            Console.WriteLine(recieverId);
            await _hubContext.Clients.User(recieverId).SendAsync("RecieveMessage",
             new
             {
                 text = data.Message,
                 id = result.MessageId,
                 createdAt = DateTime.UtcNow,
                 isSent = false,
                 senderId = userId

             });

            return Ok(new
            {
                msg = "Message sent successfully",
                id = result.MessageId,
                createdAt = DateTime.UtcNow,
                statusCode = 200
            });
        }

        [HttpGet("{friendShipId}")]
        [Authorize]
        public async Task<ActionResult> GetMessages(int friendShipId, int skip = 0,int take = 0)
        {
            var userId = HttpContext.User.FindFirst("Id")?.Value;
            var messages = await _chatRepository.GetMessages(friendShipId, userId, skip,take);
            return Ok(messages);
        }

        [HttpGet]
        [Authorize]
        [Route("all-messages")]
        public async Task<ActionResult<AllMessagesVm>> GetAllMessages()
        {
            var userId = HttpContext.User.FindFirst("Id")?.Value;
            var allMessages = await _chatRepository.GetAllMessages(userId);
            return Ok(allMessages);
        }
    }
}