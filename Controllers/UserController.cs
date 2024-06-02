using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
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
        private readonly Cloudinary _cloudinary;



        public UserController(IUserRepository userRepository, SignInManager<User> signinManager,
        IConfiguration config, UserManager<User> userManager, IFriendShipRepository friendShipRepository,
        Cloudinary cloudinary)
        {
            _userRepository = userRepository;
            _signinManager = signinManager;
            _config = config;
            _userManager = userManager;
            _friendShipRepository = friendShipRepository;
            _cloudinary = cloudinary;
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
                expires: DateTime.Now.AddDays(2),
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
                expires: DateTime.Now.AddDays(2),
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

        [HttpPost]
        [Authorize]
        [Route("image")]
        public async Task<ActionResult> Image(UploadImageVm data)
        {
            var userId = HttpContext.User.FindFirst("Id")?.Value;
            var Image = data.Image;
            var uploadResult = new ImageUploadResult();
            using (var stream = Image.OpenReadStream())
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(Image.FileName, stream)
                };

                uploadResult = await _cloudinary.UploadAsync(uploadParams);
            }

            if(uploadResult.StatusCode != HttpStatusCode.OK)
            return StatusCode(500,new {msg="Something went wrong",statusCode=500});

            var result = await _userRepository.UploadProfilePicture(userId,uploadResult.Url.ToString());

            if(!result.Succeeded)
            return  StatusCode(500,new {msg=result.ErrorMessage,statusCode=500});

            return Ok(new {msg="Profile picture updated successfully",StatusCode=200});
        }

    }
}