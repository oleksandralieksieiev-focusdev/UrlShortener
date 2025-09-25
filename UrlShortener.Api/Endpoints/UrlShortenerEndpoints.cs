using Microsoft.AspNetCore.Mvc;
using UrlShortener.Api.Services;
using UrlShortener.Api.Models;

namespace UrlShortener.Api.Endpoints;

public static class UrlShortenerEndpoints
{
    public static void MapUrlShortenerEndpoints(this WebApplication app)
    {
        // Create short URL endpoint
        app.MapPost("/shorten", async (
            [FromBody] ShortenUrlRequest request,
            IUrlShortenerService urlShortenerService) =>
        {
            try
            {
                var shortenedUrl = await urlShortenerService.ShortenUrlAsync(
                    request.LongUrl,
                    request.ExpiresAt
                );

                return Results.Ok(new
                {
                    ShortCode = shortenedUrl.ShortCode,
                    ShortUrl = $"{app.Urls.First()}/{shortenedUrl.ShortCode}"
                });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .Produces(200)
        .Produces(400)
        .WithName("ShortenUrl")
        .WithTags("UrlShortener");

        // Redirect endpoint
        app.MapGet("/{shortCode}", async (
            string shortCode,
            HttpContext context,
            IUrlShortenerService urlShortenerService) =>
        {
            var url = await urlShortenerService.GetByShortCodeAsync(shortCode);

            if (url == null)
                return Results.NotFound();

            // Record visit
            _ = urlShortenerService.RecordVisitAsync(
                shortCode,
                context.Request.Headers.Referer.ToString(),
                context.Request.Headers.UserAgent.ToString(),
                context.Connection.RemoteIpAddress?.ToString()
            );

            return Results.Redirect(url.LongUrl);
        })
        .WithName("RedirectShortUrl")
        .WithTags("UrlShortener");

        // Analytics endpoints
        app.MapGet("/analytics/top", async (
            IUrlShortenerService urlShortenerService,
            int count = 10) =>
        {
            var topUrls = await urlShortenerService.GetTopUrlsAsync(count);
            return Results.Ok(topUrls);
        })
        .WithName("GetTopUrls")
        .WithTags("Analytics");
    }

    // Request model for shortening URL
    public record ShortenUrlRequest(
        string LongUrl,
        DateTime? ExpiresAt = null
    );
}
