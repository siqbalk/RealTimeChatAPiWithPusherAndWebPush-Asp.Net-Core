using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OnlineChat.Data;
using OnlineChat.Models;
using OnlineChat.OptionModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WebPush;
using PushSubscription = OnlineChat.Models.PushSubscription;
namespace OnlineChat.Services
{
    public class PushService : IPushService
    {
        private readonly OnlineChatDbContext _context;
        private readonly WebPushClient _client;

        public WebPushOption Options { get; }

        private readonly VapidDetails _vapidDetails;

        /// <inheritdoc />
        public PushService(OnlineChatDbContext context,
            string vapidSubject, string vapidPublicKey, string vapidPrivateKey)
        {
            _context = context;
            _client = new WebPushClient();

            CheckOrGenerateVapidDetails(vapidSubject, vapidPublicKey, vapidPrivateKey);

            _vapidDetails = new VapidDetails(vapidSubject, vapidPublicKey, vapidPrivateKey);
        }

        /// <inheritdoc />
        public PushService(OnlineChatDbContext context,
            IOptions<WebPushOption> optionsAccessor)
        {
            _context = context;
            _client = new WebPushClient();
            Options = optionsAccessor.Value;

            var vapidSubject = Options.Vapidsubject;
            var vapidPublicKey = Options.VapidpublicKey;
            var vapidPrivateKey = Options.VapidprivateKey;

            CheckOrGenerateVapidDetails(vapidSubject, vapidPublicKey, vapidPrivateKey);

            _vapidDetails = new VapidDetails(vapidSubject, vapidPublicKey, vapidPrivateKey);
        }

        /// <inheritdoc />
        public void CheckOrGenerateVapidDetails(string vapidSubject, string vapidPublicKey, string vapidPrivateKey)
        {
            if (string.IsNullOrEmpty(vapidSubject) ||
                string.IsNullOrEmpty(vapidPublicKey) ||
                string.IsNullOrEmpty(vapidPrivateKey))
            {
                var vapidKeys = VapidHelper.GenerateVapidKeys();

                // Prints 2 URL Safe Base64 Encoded Strings
                Debug.WriteLine($"Public {vapidKeys.PublicKey}");
                Debug.WriteLine($"Private {vapidKeys.PrivateKey}");

                throw new Exception(
                    "You must set the Vapid:Subject, Vapid:PublicKey and Vapid:PrivateKey application settings or pass them to the service in the constructor. You can use the ones just printed to the debug console.");
            }
        }

        /// <inheritdoc />
        public string GetVapidPublicKey() => _vapidDetails.PublicKey;

        /// <inheritdoc />
        public async Task<PushSubscription> Subscribe(PushSubscription subscription)
        {
            if (await _context.pushSubscriptions.AnyAsync(s => s.P256Dh == subscription.P256Dh))

                return await _context.pushSubscriptions.FindAsync(subscription.P256Dh);

            await _context.pushSubscriptions.AddAsync(subscription);
            await _context.SaveChangesAsync();

            return subscription;
        }

        /// <inheritdoc />
        public async Task Unsubscribe(PushSubscription subscription)
        {
            if (!await _context.pushSubscriptions.AnyAsync(s => s.P256Dh == subscription.P256Dh)) return;

            _context.pushSubscriptions.Remove(subscription);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task Send(string userId, Notification notification)
        {
            foreach (var subscription in await GetUserSubscriptions(userId))
                try
                {
                    _client.SendNotification(subscription.ToWebPushSubscription(), JsonConvert.SerializeObject(notification), _vapidDetails);
                }
                catch (WebPushException e)
                {
                    if (e.Message == "Subscription no longer valid")
                    {
                        _context.pushSubscriptions.Remove(subscription);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        // Track exception with eg. AppInsights
                    }
                }
        }

        /// <inheritdoc />
        public async Task Send(string userId, string text)
        {
            await Send(userId, new Notification(text));
        }

        /// <summary>
        /// Loads a list of user subscriptions from the database
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>List of subscriptions</returns>
        private async Task<List<PushSubscription>> GetUserSubscriptions(string userId) =>
            await _context.pushSubscriptions.Where(s => s.appUser.Id == userId).ToListAsync();
    }
}
