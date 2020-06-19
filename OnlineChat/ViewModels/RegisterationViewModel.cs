using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineChat.ViewModels
{
    public class RegisterationViewModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string  FullName { get; set; }
        public string  PhoneNo { get; set; }
        public string  City { get; set; }
    }
}
