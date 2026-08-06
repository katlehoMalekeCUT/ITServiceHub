using Microsoft.EntityFrameworkCore;
using ServiceHub_IT.Models;

namespace ServiceHub_IT.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<NotificationItem> Notifications => Set<NotificationItem>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationItem>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.UserId).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<TicketComment>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TicketId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.UserName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(4000).IsRequired();
        });
    }
}
