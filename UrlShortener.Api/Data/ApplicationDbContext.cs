using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Models;

namespace UrlShortener.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) {}

    public DbSet<ShortenedUrl> ShortenedUrls => Set<ShortenedUrl>();
    public DbSet<UrlVisit> UrlVisits => Set<UrlVisit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShortenedUrl>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.ShortCode).IsUnique();
            builder.Property(x => x.ShortCode).HasMaxLength(50);
        });

        modelBuilder.Entity<UrlVisit>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.HasOne(x => x.ShortenedUrl)
                   .WithMany(x => x.Visits)
                   .HasForeignKey(x => x.ShortenedUrlId);
        });
    }
}
