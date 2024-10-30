using Microsoft.EntityFrameworkCore;

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
            optionsBuilder.UseNpgsql(
                "Host=queue_bot_db;Port=5432;Database=queuebotdb;Username=queuebot;Password=postgres");
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<SerialNumber>()
            //    .HasOne(sn => sn.Subject)
            //    .WithMany(sb => sb.SerialNumbers)
            //    .HasForeignKey(sn => sn.SubjectId);
        }
    }
}
