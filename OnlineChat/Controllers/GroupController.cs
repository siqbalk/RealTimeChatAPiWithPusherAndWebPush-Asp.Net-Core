using System.Collections.Generic;
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
    public class GroupController : ControllerBase
    {
        private UserManager<AppUser> _userManager;
        private OnlineChatDbContext _context;
        private ClaimsPrincipal _caller;

        public PusherOption Options { get; }
        private Pusher pusher;
        public GroupController(UserManager<AppUser> userManager,
            OnlineChatDbContext context,
            IHttpContextAccessor httpContextAccessor,
            IOptions<PusherOption> optionsAccessor)
        {
            _userManager = userManager;
            _context = context;
            _caller = httpContextAccessor.HttpContext.User;
            Options = optionsAccessor.Value;

            var options = new PusherOptions();
            options.Cluster = "ap2";
            options.Encrypted = true;
            pusher = new Pusher(
              Options.PUSHER_APP_ID,
              Options.PUSHER_APP_KEY,
              Options.PUSHER_APP_SECRET, options);
        }

        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            List<UserGroupViewModel> List = new List<UserGroupViewModel>();
            UserGroupViewModel model = null;
            var userId = _caller.Claims.Single(c => c.Type == "id");
            var currentUser = _userManager.FindByIdAsync(userId.Value).Result;
            var groups = _context.UserGroups.Include(p => p.Group)
                .Where(p => p.UserName == currentUser.FullName).ToList();

            foreach (var group in groups)
            {
                model = new UserGroupViewModel()
                {
                    UserName = group.UserName,
                    GroupId = group.Group.GroupId,
                    GroupName = group.Group.GroupName
                };
                List.Add(model);
            }

            return new OkObjectResult(List);
        }


        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] NewGroupViewModel group)
        {
            if (group == null || group.GroupName == "")
            {
                return new ObjectResult(new { status = "error", message = "incomplete request" });
            }

            if ((_context.Groups.Any(gp => gp.GroupName == group.GroupName)) == true)
            {
                return new ObjectResult(new { status = "error", message = "group name already exist" });
            }

            Group newGroup = new Group { GroupName = group.GroupName };
            // Insert this new group to the database...
            _context.Groups.Add(newGroup);
            _context.SaveChanges();

            //Insert into the user group table, group_id and user_id in the user_groups table...
            foreach (string UserName in group.UserNames)
            {
                _context.UserGroups.Add(new UserGroup
                {
                    UserName = UserName,
                    Group = _context.Groups.
                    FirstOrDefault(p => p.GroupName == group.GroupName)
                });
                _context.SaveChanges();
            }

            var result = await pusher.TriggerAsync(
                "group_chat", //channel name
                "new_group", // event name
            new { newGroup });


            return new OkObjectResult(new { status = "success", data = newGroup });
        }


    }
}