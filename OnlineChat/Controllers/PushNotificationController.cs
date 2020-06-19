using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineChat.Models;
using OnlineChat.Services;
using OnlineChat.ViewModels;

namespace OnlineChat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PushNotificationController : ControllerBase
    {

        private readonly IHostingEnvironment _env;
        private readonly IPushService _pushService;
        private readonly UserManager<AppUser> _userManager;

        /// <inheritdoc />
        public PushNotificationController(IHostingEnvironment hostingEnvironment,
            IPushService pushService , UserManager<AppUser> userManager)
        {
            _env = hostingEnvironment;
            _pushService = pushService;
            _userManager = userManager;
        }

        // GET: api/push/vapidpublickey
        /// <summary>
        /// Get VAPID Public Key
        /// </summary>
        /// <returns>VAPID Public Key</returns>
        /// <response code="200">OK</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet("vapidpublickey")]
        public ActionResult<string> GetVapidPublicKey()
        {
            return Ok(_pushService.GetVapidPublicKey());
        }

        // POST: api/push/subscribe
        /// <summary>
        /// Subscribe for push notifications

        [HttpPost("subscribe")]
        public async Task<ActionResult<PushSubscription>> Subscribe([FromBody] PushSubscriptionViewModel model)
        {
            var subscription = new PushSubscription
            {
                appUser = _userManager.FindByIdAsync(model.Subscription.UserId).Result,
                Endpoint = model.Subscription.Endpoint,
                ExpirationTime = model.Subscription.ExpirationTime,
                Auth = model.Subscription.Keys.Auth,
                P256Dh = model.Subscription.Keys.P256Dh
            };

            return await _pushService.Subscribe(subscription);
        }

        // POST: api/push/unsubscribe
        /// <summary>
        /// Unsubscribe for push notifications
        /// </summary>
        /// <returns>No content</returns>
        /// <response code="204">NoContent</response>
        /// <response code="400">BadRequest if subscription is null or invalid.</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost("unsubscribe")]
        public async Task<ActionResult<PushSubscription>> Unsubscribe([FromBody] PushSubscriptionViewModel model)
        {
            var subscription = new PushSubscription
            {
                Endpoint = model.Subscription.Endpoint,
                ExpirationTime = model.Subscription.ExpirationTime,
                Auth = model.Subscription.Keys.Auth,
                P256Dh = model.Subscription.Keys.P256Dh
            };

            await _pushService.Unsubscribe(subscription);

            return subscription;
        }

        // POST: api/push/send
        /// <summary>
        /// Send a push notifications to a specific user's every device (for development only!)
        /// </summary>
        /// <returns>No content</returns>
        /// <response code="202">Accepted</response>
        /// <response code="400">BadRequest if subscription is null or invalid.</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost("send/{userId}")]
        public async Task<ActionResult<AcceptedResult>> Send([FromRoute] string userId, [FromBody] Notification notification, [FromQuery] int? delay)
        {
            if (!_env.IsDevelopment()) return Forbid();

            if (delay != null) Thread.Sleep((int)delay);

            await _pushService.Send(userId, notification);

            return Accepted();
        }


        [HttpPost("SendSimpleNotification")]
        public async Task<IActionResult> SendSimpleNotification([FromBody]string userId , string text)
        {
           
            await _pushService.Send(userId , text);

            return Accepted();
        }
    }

}