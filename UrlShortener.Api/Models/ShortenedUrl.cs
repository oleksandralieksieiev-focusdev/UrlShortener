using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Api.Models;

public class ShortenedUrl
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(2048)]
    public string LongUrl { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string ShortCode { get; set; } = string.Empty;

    public int ClickCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public List<UrlVisit> Visits { get; set; } = new();
}

public class UrlVisit
{
    public Guid Id { get; set; }
    public Guid ShortenedUrlId { get; set; }
    public ShortenedUrl? ShortenedUrl { get; set; }
    public string? Referrer { get; set; }
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }
    public DateTime VisitedAt { get; set; }
}
