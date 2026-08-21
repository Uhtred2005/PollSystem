using Microsoft.EntityFrameworkCore;
using VoteService.Models;

namespace VoteService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Vote> Votes => Set<Vote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Vote>(entity =>
        {
            // 1. Chỉ định khóa chính
            entity.HasKey(e => e.Id);

            // 2. Composite Unique Index: Ràng buộc cốt lõi chống gian lận
            // Đảm bảo cặp giá trị (PollCode + VoterToken) luôn duy nhất trong toàn bộ bảng
            entity.HasIndex(e => new { e.PollCode, e.VoterToken })
                  .IsUnique();

            // 3. Đánh index phụ trên PollCode để tăng tốc truy vấn đếm số lượng vote
            entity.HasIndex(e => e.PollCode);

            // 4. Các ràng buộc độ dài để tối ưu không gian lưu trữ
            entity.Property(e => e.PollCode)
                  .IsRequired()
                  .HasMaxLength(10); // Code chỉ 6 ký tự, giới hạn max 10 là đủ

            entity.Property(e => e.VoterToken)
                  .IsRequired()
                  .HasMaxLength(255); // Browser fingerprint hash thường không quá 255 ký tự
        });
    }
}