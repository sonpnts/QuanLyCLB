namespace QuanLyCLB.Application.Entities;

public class LoginAuditLog : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? UserAccountId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Provider { get; set; } = string.Empty;

    public bool IsSuccess { get; set; }

    public string ApiEndpoint { get; set; } = string.Empty;

    public string? LocationAddress { get; set; }

    public string? DeviceInfo { get; set; }

    public string? IpAddress { get; set; }

    public string? Message { get; set; }

    public UserAccount? UserAccount { get; set; }
}
