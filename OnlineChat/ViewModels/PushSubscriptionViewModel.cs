using OnlineChat.ViewModels.PushNotificationModel;

namespace OnlineChat.ViewModels
{
    public class PushSubscriptionViewModel
    {
        /// <inheritdoc cref="Subscription"/>
        public Subscription Subscription { get; set; }

        /// <summary>
        /// Other attributes, like device id for example.
        /// </summary>
        public string DeviceId { get; set; }
    }
}
