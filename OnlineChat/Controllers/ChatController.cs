using System;
using System.Linq;
using System.Security.Claims;
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
    public class ChatController : ControllerBase
    {
        private UserManager<AppUser> _userManager;
        private OnlineChatDbContext _context;
        private ClaimsPrincipal _caller;

        public PusherOption Options { get; }

        private Pusher pusher;
        public ChatController(UserManager<AppUser> userManager,
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
            pusher = new Pusher(
              Options.PUSHER_APP_ID,
              Options.PUSHER_APP_KEY,
              Options.PUSHER_APP_SECRET, options);
        }

        [HttpGet("allUsers")]

        public ActionResult GetAllUser()
        {
            var userId = _caller.Claims.Single(c => c.Type == "id");
            var currentUser = _userManager.FindByIdAsync(userId.Value).Result;

            var allUser = _userManager.Users.Where(p => p.Id != currentUser.Id).ToList();


            return new OkObjectResult(allUser);
        }

        [HttpGet("CurrentUser")]
        public ActionResult GetCurrentUser()
        {
            var userId = _caller.Claims.Single(c => c.Type == "id");
            var currentUser = _userManager.FindByIdAsync(userId.Value).Result;




            return new OkObjectResult(currentUser);
        }

      
        [HttpGet("ConversationWithContact/{contact}")]
        public IActionResult ConversationWithContact(string contact)
        {

            var userId = _caller.Claims.Single(c => c.Type == "id");
            var currentUser = _userManager.FindByIdAsync(userId.Value).Result;


            var conversations = _context.conversations.
                               Where(c => (c.receiver_id == currentUser.Id &&
                               c.sender_id == contact) || (c.receiver_id
                               == contact && c.sender_id == currentUser.Id))
                               .OrderBy(c => c.created_at)
                               .ToList();

            return new OkObjectResult(conversations);
        }

        [HttpPost("SendMessage")]
        public IActionResult SendMessage([FromBody] MessageSendViewModel model)
        {
            var userId = _caller.Claims.Single(c => c.Type == "id");
            var currentUser = _userManager.FindByIdAsync(userId.Value).Result;

            //var contact = Request.Form["contact"].ToString();
            //string socket_id = Request.Form["socket_id"];
            Conversation convo = new Conversation
            {
                sender_id = currentUser.Id,
                //message = Request.Form["message"],
                message= model.Message,
                receiver_id = model.contact
            };

            _context.Add(convo);
            _context.SaveChanges();

            var conversationChannel = getConvoChannel(currentUser.Id, model.contact);
            pusher.TriggerAsync(
              conversationChannel,
              "new_message",
              convo
              //new TriggerOptions() { SocketId = socket_id }
              );


            return new OkObjectResult(convo);
        }

        [HttpPost("MessageDelivered/{message_id}")]

        public IActionResult MessageDelivered(int message_id)
        {


            var convo = _context.conversations.FirstOrDefault(c => c.ConversationId == message_id);
            if (convo != null)
            {
                convo.status = Conversation.messageStatus.Delivered;
                _context.Entry(convo).State = EntityState.Modified;
                _context.SaveChanges();
            }


            //string socket_id = Request.Form["socket_id"];
            var conversationChannel = getConvoChannel(convo.sender_id, convo.receiver_id);
            pusher.TriggerAsync(
              conversationChannel,
              "message_delivered",
              convo
              //new TriggerOptions() { SocketId = socket_id }
              );
            return new OkObjectResult(convo);
        }
        private String getConvoChannel(string user_id, string contact_id)
        {
            if (user_id.CompareTo(contact_id) > 0)
            {
                return "private-chat-" + contact_id + "-" + user_id;
            }
            return "private-chat-" + user_id + "-" + contact_id;
        }


    }
}