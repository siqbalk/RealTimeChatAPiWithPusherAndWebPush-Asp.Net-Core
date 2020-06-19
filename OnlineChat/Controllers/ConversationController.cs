using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineChat.Data;
using OnlineChat.Models;

namespace OnlineChat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConversationController : ControllerBase
    {
        private OnlineChatDbContext _context;
        private ClaimsPrincipal _caller;
        private UserManager<AppUser> _userManager;

        public ConversationController(OnlineChatDbContext context,
            UserManager<AppUser> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _caller = httpContextAccessor.HttpContext.User;
            _userManager = userManager;
        }

        [HttpGet("{contact}")]
        public IActionResult Get(string contact)
        {
            var userId = _caller.Claims.Single(c => c.Type == "id");
            var currentUser = _userManager.FindByIdAsync(userId.Value).Result;




            var conversations = _context.conversations.
                               Where(c => (c.receiver_id == currentUser.Id
                               && c.sender_id == contact) || (c.receiver_id ==
                               contact && c.sender_id == currentUser.Id))
                               .OrderBy(c => c.created_at)
                               .ToList();

            return new OkObjectResult(conversations);
        }
    }
}