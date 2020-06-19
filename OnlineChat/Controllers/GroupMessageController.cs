using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnlineChat.Data;
using OnlineChat.Models;
using OnlineChat.PusherModel;
using OnlineChat.ViewModels;
using PusherServer;

namespace OnlineChat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupMessageController : ControllerBase
    {
        private readonly OnlineChatDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly ClaimsPrincipal _caller;

        public PusherOption Options { get; }
        private Pusher pusher;
        public GroupMessageController(OnlineChatDbContext context,
            UserManager<AppUser> userManager,
             IHttpContextAccessor httpContextAccessor,
             IOptions<PusherOption> optionsAccessor)
        {
            _context = context;
            _userManager = userManager;
            _caller = httpContextAccessor.HttpContext.User;
            Options = optionsAccessor.Value;

            var options = new PusherOptions();
            options.Cluster = "ap2";
            pusher = new Pusher(
              Options.PUSHER_APP_ID,
              Options.PUSHER_APP_KEY,
              Options.PUSHER_APP_SECRET, options);
        }
    

        [HttpGet("GetById/{group_id}")]
        public IActionResult GetById(int group_id)
        {
            var groupMessages= _context.GroupMessages.Include(p=>p.Group)
                .Where(gb => gb.Group.GroupId == group_id);

            return new  OkObjectResult(groupMessages);
        }


        [HttpPost("GroupMessageSend")]
        public async Task<IActionResult> GroupMessageSend([FromBody] GroupMessageSendViewModel message)
        {
            var userId = _caller.Claims.Single(c => c.Type == "id");
            var currentUser = _userManager.FindByIdAsync(userId.Value).Result;
            GroupMessage new_message = new GroupMessage
            { AddedBy = currentUser.FullName,
                message = message.message, Group = _context.Groups.
                FirstOrDefault(p => p.GroupId == message.GroupId) };

            _context.GroupMessages.Add(new_message);
            _context.SaveChanges();

            var result = await pusher.TriggerAsync(
                "private-" + message.GroupId,
                "new_message",
            new { new_message }
            //new TriggerOptions() { SocketId = message.SocketId }
            );

            return new OkObjectResult(new { status = "success", data = new_message });
        }
    }
}