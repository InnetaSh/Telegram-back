using Microsoft.EntityFrameworkCore;

namespace Telegram_back.Models
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<ChatUser> ChatUsers { get; set; }

        public DbSet<Chat> Chats { get; set; }
        public DbSet<Message> Messages { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            //Database.EnsureDeleted();
            //Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ChatUser>()
                .HasKey(cu => new { cu.ChatId, cu.UserId });

            modelBuilder.Entity<ChatUser>()
               .HasOne(cu => cu.User)
               .WithMany(u => u.ChatUsers)
               .HasForeignKey(cu => cu.UserId);

            modelBuilder.Entity<ChatUser>()
                .HasOne(cu => cu.Chat)
                .WithMany(c => c.ChatUsers)
                .HasForeignKey(cu => cu.ChatId);

        }
    }
}
