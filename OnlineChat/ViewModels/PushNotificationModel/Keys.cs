namespace OnlineChat.ViewModels.PushNotificationModel
{
    public class Keys
    {
        /// <summary>
        /// An <see href="https://en.wikipedia.org/wiki/Elliptic_curve_Diffie%E2%80%93Hellman">Elliptic curve Diffie–Hellman</see> public key on the P-256 curve (that is, the NIST secp256r1 elliptic curve).
        /// The resulting key is an uncompressed point in ANSI X9.62 format.
        /// </summary>
        public string P256Dh { get; set; }

        /// <summary>
        /// An authentication secret, as described in <see href="https://tools.ietf.org/html/draft-ietf-webpush-encryption-08">Message Encryption for Web Push</see>.
        /// </summary>
        public string Auth { get; set; }
    }
}
