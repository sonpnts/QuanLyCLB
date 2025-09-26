using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuanLyCLB.Application.Interfaces;

namespace QuanLyCLB.Infrastructure.Services;

public class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly ILogger<GoogleTokenValidator> _logger;
    private readonly GoogleJsonWebSignature.ValidationSettings _settings;

    public GoogleTokenValidator(ILogger<GoogleTokenValidator> logger, IConfiguration configuration)
    {
        _logger = logger;
        var audience = configuration.GetSection("Authentication:Google:ClientIds").Get<string[]>() ?? Array.Empty<string>();
        _settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = audience
        };
    }

    public async Task<GoogleUserInfo?> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, _settings);
            return new GoogleUserInfo(payload.Subject, payload.Email, payload.Name);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to validate Google token");
            return null;
        }
    }
}
