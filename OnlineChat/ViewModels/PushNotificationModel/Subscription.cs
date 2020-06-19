namespace OnlineChat.ViewModels.PushNotificationModel
{
    public class Subscription
    {
        /// <summary>
        /// The endpoint associated with the push subscription.
        /// </summary>
        public string Endpoint { get; set; }

        public string  UserId { get; set; }

        /// <summary>
        /// The subscription expiration time associated with the push subscription, if there is one, or null otherwise.
        /// </summary>
        public double? ExpirationTime { get; set; }

        /// <inheritdoc cref="Keys"/>
        public Keys Keys { get; set; }

        /// <summary>
        /// Converts the push subscription to the format of the library WebPush
        /// </summary>
        /// <returns>WebPush subscription</returns>
        public WebPush.PushSubscription ToWebPushSubscription() => new WebPush.PushSubscription(Endpoint, Keys.P256Dh, Keys.Auth);
    }
}
