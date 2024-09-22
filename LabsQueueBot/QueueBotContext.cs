using Microsoft.EntityFrameworkCore;

namespace LabsQueueBot
{
    public class QueueBotContext : DbContext
    {
        public DbSet<User> UserRepository { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(
                "Host=localhost;Port=5432;Database=QueueBotDB;Username=postgres;Password=postgres");
        }
    }
}
