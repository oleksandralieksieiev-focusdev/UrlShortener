# C# URL Shortener

## Tech stack:
- PostgreSQL
- ASP.NET
- C#
- (optionally) Redis
- Docker
- Swagger

## Running
```bash
dotnet build
dotnet run
```

You might need to tinker around with the migrations.

### API routes
This is just the backend.

- `GET /swagger` - API docs
- `POST /shorten` - shorten a new URL
    - `longUrl: string` - original URL
    - `expiresAt: string` - date in ISO format
- `GET /{shortcode}` - get redirected to `longUrl`
- `GET /analytics/top?count={n}` - Get the top `n` links.
    - Default is 10 if no `count` provided.
