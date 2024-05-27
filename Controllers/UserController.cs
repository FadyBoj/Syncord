using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Syncord.Models;
using Syncord.Repositories;
using Syncord.ViewModels;

namespace Syncord.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly SignInManager<User> _signinManager;
        private readonly IConfiguration _config;
        private readonly UserManager<User> _userManager;
        private readonly IFriendShipRepository _friendShipRepository;



        public UserController(IUserRepository userRepository, SignInManager<User> signinManager,
        IConfiguration config, UserManager<User> userManager, IFriendShipRepository friendShipRepository)
        {
            _userRepository = userRepository;
            _signinManager = signinManager;
            _config = config;
            _userManager = userManager;
            _friendShipRepository = friendShipRepository;
        }

        [HttpPost]
        [Route("register")]
        public async Task<ActionResult> SayHello(RegisterVm user)
        {
            var existingUser = await _userRepository.GetUserByEmail(user.Email);
            if (existingUser != null)
                return BadRequest("This email is already taken");

            var result = await _userRepository.AddUser(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            var addedUser = await _userRepository.GetUserByEmail(user.Email);

            //Generating jwt 
            var jwtIssuer = _config["Jwt:Issuer"];
            var jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var jwtCredentials = new SigningCredentials(jwtKey, SecurityAlgorithms.HmacSha256);

            var userClaims = new[]{
                new Claim("Id",addedUser.Id),
                new Claim(ClaimTypes.Email,addedUser.Email)
            };

            var secToken = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtIssuer,
                claims: userClaims,
                expires: DateTime.Now.AddMinutes(100),
                signingCredentials: jwtCredentials
            );

            var token = new JwtSecurityTokenHandler().WriteToken(secToken);

            return Ok(new
            {
                msg = "Your account has been created successfully",
                token = token
            });
        }

        [HttpPost]
        [Route("login")]

        public async Task<ActionResult> Login(LoginVm credentials)
        {
            if (credentials.Email == null || credentials.Password == null)
                return BadRequest("All fields must be provided");

            var user = await _userRepository.GetUserByEmail(credentials.Email);
            if (user == null)
                return BadRequest("Email and password are mismatched");

            var loginResult = await _signinManager.PasswordSignInAsync(user, credentials.Password, false, false);

            if (!loginResult.Succeeded)
                return BadRequest("Email and password are mismatched");

            var jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var jwtCredentials = new SigningCredentials(jwtKey, SecurityAlgorithms.HmacSha256);
            var jwtIssuer = _config["Jwt:Issuer"];

            var userClaims = new[]{
                new Claim(ClaimTypes.Email,user.Email),
                new Claim("Id",user.Id)
            };

            var secToken = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtIssuer,
                claims: userClaims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: jwtCredentials
            );

            var token = new JwtSecurityTokenHandler().WriteToken(secToken);

            return Ok(new { token = token });
        }

        [HttpGet]
        [Route("requests")]
        [Authorize]
        public async Task<ActionResult> GetData()
        {
            var userId = HttpContext.User.FindFirst("Id")?.Value;
            Console.WriteLine(userId);
            var requests = await _userRepository.GetRequests(userId);
            return Ok(new { requests = requests });
        }

        [HttpGet]
        [Route("Friends")]
        [Authorize]
        public async Task<ActionResult> Friends()
        {
            var userId = HttpContext.User.FindFirst("Id")?.Value;
            var friends = await _friendShipRepository.GetFriends(userId);

            return Ok(friends);
        }

        [HttpPost]
        [Route("check-user-exist")]
        public async Task<ActionResult> CheckUserExist(CheckExistVm data)
        {
            var result = await _userRepository.IsUserExist(data.Email);
            return Ok(result);
        }

        [HttpGet]
        [Authorize]
        [Route("dashboard")]
        public async Task<ActionResult<DashboardVm>> Dashboard()
        {
            var userId = HttpContext.User.FindFirst("Id")?.Value;
            var dashboard = await _userRepository.Dashboard(userId);
            return Ok(dashboard);
        }

    }
}