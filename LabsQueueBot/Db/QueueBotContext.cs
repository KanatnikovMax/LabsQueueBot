using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LabsQueueBot
{
    public class QueueBotContext : DbContext
    {
        public DbSet<User> UserRepository { get; set; }

        public DbSet<Subject> SubjectRepository { get; set; }

        public DbSet<SerialNumber> SerialNumberRepository { get; set; }

        public QueueBotContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Development.json", optional: false)
                .Build();
            string connectionString = configuration.GetValue<string>("HotelChainDbContext");
            optionsBuilder.UseNpgsql(connectionString);
        }
    }
}
