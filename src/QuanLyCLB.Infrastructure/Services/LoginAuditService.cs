using QuanLyCLB.Application.Entities;
using QuanLyCLB.Application.Interfaces;
using QuanLyCLB.Infrastructure.Persistence;

namespace QuanLyCLB.Infrastructure.Services;

public class LoginAuditService : ILoginAuditService
{
    private readonly ClubManagementDbContext _dbContext;

    public LoginAuditService(ClubManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task RecordAsync(
        Guid? userAccountId,
        string username,
        string provider,
        bool isSuccess,
        string apiEndpoint,
        string? locationAddress,
        string? deviceInfo,
        string? ipAddress,
        string? message,
        CancellationToken cancellationToken = default)
    {
        var entry = new LoginAuditLog
        {
            UserAccountId = userAccountId,
            Username = username,
            Provider = provider,
            IsSuccess = isSuccess,
            ApiEndpoint = apiEndpoint,
            LocationAddress = locationAddress,
            DeviceInfo = deviceInfo,
            IpAddress = ipAddress,
            Message = message,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.LoginAuditLogs.AddAsync(entry, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
