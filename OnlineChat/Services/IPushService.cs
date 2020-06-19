using OnlineChat.Models;
using OnlineChat.OptionModel;
using System.Threading.Tasks;

namespace OnlineChat.Services
{
    public interface IPushService
    {
        WebPushOption Options { get; }

        /// Checks VAPID info and if invalid generates new keys and throws exception
        /// </summary>
        /// <param name="subject">This should be a URL or a 'mailto:' email address</param>
        /// <param name="vapidPublicKey">The VAPID public key as a base64 encoded string</param>
        /// <param name="vapidPrivateKey">The VAPID private key as a base64 encoded string</param>
        void CheckOrGenerateVapidDetails(string vapidSubject, string vapidPublicKey, string vapidPrivateKey);

        // Get the server's saved VAPID public key
        string GetVapidPublicKey();


        // Send a push notification to a user
        Task Send(string userId, Notification notification);

        // Send a plain text push notification to a user without any special option
        Task Send(string userId, string text);

        // Register a push subscription (save to the database for later use)
        Task<PushSubscription> Subscribe(PushSubscription subscription);


        // Un-register a push subscription (delete from the database)
        Task Unsubscribe(PushSubscription subscription);
    }
}