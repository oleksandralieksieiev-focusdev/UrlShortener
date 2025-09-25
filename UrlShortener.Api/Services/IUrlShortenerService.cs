using UrlShortener.Api.Models;

namespace UrlShortener.Api.Services;

public interface IUrlShortenerService
{
    Task<ShortenedUrl> ShortenUrlAsync(string longUrl, DateTime? expiresAt = null);
    Task<ShortenedUrl?> GetByShortCodeAsync(string shortCode);
    Task<bool> RecordVisitAsync(string shortCode, string? referrer, string? userAgent, string? ipAddress);
    Task<List<ShortenedUrl>> GetTopUrlsAsync(int count = 10);
}
