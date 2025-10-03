namespace QuanLyCLB.Application.Interfaces;

public interface ILoginAuditService
{
    Task RecordAsync(
        Guid? userAccountId,
        string username,
        string provider,
        bool isSuccess,
        string apiEndpoint,
        string? locationAddress,
        string? deviceInfo,
        string? ipAddress,
        string? message,
        CancellationToken cancellationToken = default);
}
