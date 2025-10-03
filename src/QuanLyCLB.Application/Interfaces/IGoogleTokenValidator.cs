namespace QuanLyCLB.Application.Interfaces;

public interface IGoogleTokenValidator
{
    Task<GoogleUserInfo?> ValidateAsync(string idToken, CancellationToken cancellationToken = default);
}

public record GoogleUserInfo(string Subject, string Email, string Name, string AvatarUrl);
