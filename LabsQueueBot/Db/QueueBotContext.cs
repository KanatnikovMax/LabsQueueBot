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
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .Build();
            string connectionString = configuration.GetValue<string>("QueueBotDbContext");
            optionsBuilder.UseNpgsql(connectionString);
        }
    }
}
