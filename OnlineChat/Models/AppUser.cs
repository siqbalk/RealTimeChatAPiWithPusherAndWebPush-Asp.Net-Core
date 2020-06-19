using Microsoft.AspNetCore.Identity;
using System;

namespace OnlineChat.Models
{
    public class AppUser:IdentityUser
    {
        public string  FullName { get; set; }
        public string  City { get; set; }
        public byte[] ProfileImage { get; set; }
        public DateTime   Created { get; set; }

    }
}
