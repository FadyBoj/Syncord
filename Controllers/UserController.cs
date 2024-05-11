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


        public UserController(IUserRepository userRepository, SignInManager<User> signinManager,
        IConfiguration config)
        {
            _userRepository = userRepository;
            _signinManager = signinManager;
            _config = config;
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

            return Ok("Your account has been created successfully");
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

      
    }
}