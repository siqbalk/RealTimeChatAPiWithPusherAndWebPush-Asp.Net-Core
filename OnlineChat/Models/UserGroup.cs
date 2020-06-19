using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineChat.Models
{
    [Table("UserGroup")]
    public class UserGroup
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int UserGroupId { get; set; }

        public string UserName { get; set; }
        public Group Group { get; set; }

    }
}
