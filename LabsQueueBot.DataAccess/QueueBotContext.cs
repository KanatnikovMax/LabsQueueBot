using LabsQueueBot.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace LabsQueueBot.DataAccess;

public class QueueBotContext : DbContext
{
    public DbSet<User> UserRepository { get; set; }
    public DbSet<Subject> SubjectRepository { get; set; }
    public DbSet<Baned> BlackListRepository { get; set; }

    public QueueBotContext(DbContextOptions options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasKey(u => u.Id);
        modelBuilder.Entity<User>().HasIndex(u => u.Username)
            .IsUnique();
        
        modelBuilder.Entity<Subject>().HasKey(s => s.Id);
        modelBuilder.Entity<Subject>().HasIndex(s => new { s.CourseNumber, s.GroupNumber, s.SubjectName })
            .IsUnique();

        modelBuilder.Entity<Baned>().HasKey(b => b.Id);
        modelBuilder.Entity<Baned>().HasIndex(b => new { b.UserId, b.SubjectId })
            .IsUnique();
    }
}