using Microsoft.EntityFrameworkCore;

namespace LabsQueueBot
{
    public class QueueBotContext : DbContext
    {
        public DbSet<User> UserRepository { get; set; }

        public DbSet<Subject> SubjectRepository { get; set; }

        public DbSet<SerialNumber> SerialNumberRepository { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(
                "Host=localhost;Port=5432;Database=QueueBotDB;Username=postgres;Password=postgres");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SerialNumber>()
                .HasOne(sn => sn.Subject)
                .WithMany(sb => sb.SerialNumbers)
                .HasForeignKey(sn => sn.SubjectId);

            modelBuilder.Entity<SerialNumber>()
                .HasKey(sn => new { sn.Id, sn.SubjectId });
        }
    }
}
