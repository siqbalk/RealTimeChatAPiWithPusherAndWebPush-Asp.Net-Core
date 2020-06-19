using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineChat.OptionModel
{
    public class WebPushOption
    {
        public string Vapidsubject { get; set; }
        public string VapidpublicKey { get; set; }
        public string VapidprivateKey { get; set; }
    }
}
