using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineChat.Models
{
    [Table("GroupMessage")]
    public class GroupMessage
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int GroupMessageId { get; set; }
        public string AddedBy { get; set; }
        public string message { get; set; }
        public Group Group { get; set; }

    }
}
