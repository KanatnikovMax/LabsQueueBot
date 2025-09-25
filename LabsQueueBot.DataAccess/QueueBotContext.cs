using LabsQueueBot.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace LabsQueueBot.DataAccess;

public class QueueBotContext : DbContext
{
    public DbSet<User> UserRepository { get; set; }

    public DbSet<Subject> SubjectRepository { get; set; }

    public QueueBotContext(DbContextOptions options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasKey(u => u.Id);
        
        modelBuilder.Entity<Subject>().HasKey(s => s.Id);
        modelBuilder.Entity<Subject>().HasIndex(s => new { s.CourseNumber, s.GroupNumber, s.SubjectName })
            .IsUnique();
    }
}