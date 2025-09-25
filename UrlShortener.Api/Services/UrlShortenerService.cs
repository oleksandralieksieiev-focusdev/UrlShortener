using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Data;
using UrlShortener.Api.Models;

namespace UrlShortener.Api.Services;

public class UrlShortenerService : IUrlShortenerService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UrlShortenerService> _logger;

    public UrlShortenerService(ApplicationDbContext context, ILogger<UrlShortenerService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Custom short code generator
    private string GenerateShortCode(int length = 6)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[length];
        rng.GetBytes(bytes);

        var result = new StringBuilder(length);
        foreach (byte b in bytes)
        {
            result.Append(chars[b % chars.Length]);
        }

        return result.ToString();
    }

    public async Task<ShortenedUrl> ShortenUrlAsync(string longUrl, DateTime? expiresAt = null)
    {
        // Validate URL
        if (!Uri.TryCreate(longUrl, UriKind.Absolute, out _))
        {
            throw new ArgumentException("Invalid URL format");
        }

        // Generate unique short code
        string shortCode;
        do
        {
            shortCode = GenerateShortCode();
        } while (await _context.ShortenedUrls.AnyAsync(u => u.ShortCode == shortCode));

        var shortenedUrl = new ShortenedUrl
        {
            Id = Guid.NewGuid(),
            LongUrl = longUrl,
            ShortCode = shortCode,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt ?? DateTime.UtcNow.AddMonths(6),
            ClickCount = 0
        };

        _context.ShortenedUrls.Add(shortenedUrl);
        await _context.SaveChangesAsync();

        return shortenedUrl;
    }


    public async Task<ShortenedUrl?> GetByShortCodeAsync(string shortCode)
    {
        return await _context.ShortenedUrls
            .FirstOrDefaultAsync(u => u.ShortCode == shortCode &&
                                      (u.ExpiresAt == null || u.ExpiresAt > DateTime.UtcNow));
    }

    public async Task<bool> RecordVisitAsync(string shortCode, string? referrer, string? userAgent, string? ipAddress)
    {
        var url = await _context.ShortenedUrls
            .FirstOrDefaultAsync(u => u.ShortCode == shortCode);

        if (url == null) return false;

        // Record visit
        var visit = new UrlVisit
        {
            Id = Guid.NewGuid(),
            ShortenedUrlId = url.Id,
            Referrer = referrer,
            UserAgent = userAgent,
            IpAddress = ipAddress,
            VisitedAt = DateTime.UtcNow
        };

        // Increment click count
        url.ClickCount++;

        _context.UrlVisits.Add(visit);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<ShortenedUrl>> GetTopUrlsAsync(int count = 10)
    {
        return await _context.ShortenedUrls
            .OrderByDescending(u => u.ClickCount)
            .Take(count)
            .ToListAsync();
    }
}
