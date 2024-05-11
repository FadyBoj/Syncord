using Microsoft.AspNetCore.Mvc;
using Syncord.Repositories;
using Syncord.ViewModels;

namespace Syncord.Controllers{
    [ApiController]
    [Route("[controller]")]
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        
        [HttpPost]
        [Route("register")]
        public async Task<ActionResult> SayHello(RegisterVm user)
        {
            var existingUser = await _userRepository.GetUserByEmail(user.Email);
            if(existingUser != null)
            return BadRequest("This email is already taken");

            var result = await _userRepository.AddUser(user);

            if(!result.Succeeded)
            return BadRequest(result.Errors);

            return  Ok("Your account has been created successfully");
        }
    }
}