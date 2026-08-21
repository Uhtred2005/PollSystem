using Microsoft.EntityFrameworkCore;
using PollService.Models;

namespace PollService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Poll> Polls => Set<Poll>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Poll>(entity =>
        {
            // 1. Primary Key
            entity.HasKey(e => e.Id);

            // 2. Unique Index for Short Code
            entity.HasIndex(e => e.Code)
                  .IsUnique();

            // 3. Question constraints
            entity.Property(e => e.Question)
                  .IsRequired()
                  .HasMaxLength(500);

            // 4. Convert List<string> to string for database storage
            entity.Property(e => e.Options)
                  .HasConversion(
                      v => string.Join(';', v),
                      v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList()
                  );

            // 5. Store Enum as string
            entity.Property(e => e.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20);
        });
    }
}