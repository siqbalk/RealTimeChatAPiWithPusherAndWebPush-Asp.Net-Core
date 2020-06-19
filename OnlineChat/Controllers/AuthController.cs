using System.Security.Claims;
using System.Threading.Tasks;
using OnlineChat.Auth;
using OnlineChat.Helpers;
using OnlineChat.Models;
using OnlineChat.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Linq;
using PusherServer;
using System;

namespace OnlineChat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IJwtFactory _jwtFactory;
        private readonly JwtIssuerOptions _jwtOptions;
        private readonly ClaimsPrincipal _caller;
        private Pusher pusher;
        public AuthController(UserManager<AppUser> userManager,
            IJwtFactory jwtFactory, IOptions<JwtIssuerOptions> jwtOptions,
             IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _jwtFactory = jwtFactory;
            _jwtOptions = jwtOptions.Value;
            _caller = httpContextAccessor.HttpContext.User;

            var options = new PusherOptions();
            options.Cluster = "ap2";

            pusher = new Pusher(
                "1018644",
              "2cf32f932ef043c5f35e",
              "277084b1b4409be49fe6",
               options
           );
        }

        // POST api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Post([FromBody]CredentialsViewModel credentials)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var identity = await GetClaimsIdentity(credentials.UserName, credentials.Password);
            if (identity == null)
            {
                return BadRequest(Errors.AddErrorToModelState("login_failure", "Invalid username or password.", ModelState));
            }

            var jwt = await Tokens.GenerateJwt(identity, _jwtFactory, credentials.UserName, _jwtOptions, new JsonSerializerSettings { Formatting = Formatting.Indented });
            return new OkObjectResult(jwt);
        }

        private async Task<ClaimsIdentity> GetClaimsIdentity(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return await Task.FromResult<ClaimsIdentity>(null);

            // get the user to verifty
            var userToVerify = await _userManager.FindByNameAsync(userName);

            if (userToVerify == null) return await Task.FromResult<ClaimsIdentity>(null);

            // check the credentials
            if (await _userManager.CheckPasswordAsync(userToVerify, password))
            {
                return await Task.FromResult(_jwtFactory.GenerateClaimsIdentity(userName, userToVerify.Id));
            }

            // Credentials are invalid, or account doesn't exist
            return await Task.FromResult<ClaimsIdentity>(null);
        }



        [HttpPost("AuthForChannel/{channel_name}/{socket_id}")]
        public IActionResult AuthForChannel(string channel_name, string socket_id)
        {
            var userId = _caller.Claims.Single(c => c.Type == "id");
            var currentUser = _userManager.FindByIdAsync(userId.Value).Result;

            if (channel_name.IndexOf("presence") >= 0)
            {

                var channelData = new PresenceChannelData()
                {
                    user_id = currentUser.Id.ToString(),
                    user_info = new
                    {
                        id = currentUser.Id,
                        name = currentUser.FullName
                    },
                };

                var presenceAuth = pusher.Authenticate(channel_name, socket_id, channelData);

                return new OkObjectResult(presenceAuth);

            }

            if (channel_name.IndexOf(currentUser.Id.ToString()) == -1)
            {
                throw new ArgumentException("User cannot join channel");
            }

            var auth = pusher.Authenticate(channel_name, socket_id);
            return new OkObjectResult(auth);


        }
    }
}