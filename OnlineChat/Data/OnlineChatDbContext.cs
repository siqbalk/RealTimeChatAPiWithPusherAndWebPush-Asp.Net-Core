using OnlineChat.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace OnlineChat.Data
{
    public class OnlineChatDbContext : IdentityDbContext<AppUser>
    {
        public OnlineChatDbContext(DbContextOptions<OnlineChatDbContext> options)
              : base(options)
        {
        }

        public DbSet<Conversation>  conversations { get; set; }
        public DbSet<Group>  Groups { get; set; }

        public DbSet<GroupMessage>  GroupMessages { get; set; }

        public DbSet<UserGroup>  UserGroups { get; set; }

        public DbSet<PushSubscription> pushSubscriptions { get; set; }

        //public DbSet<Customer> Customers { get; set; }
    }
}
