using System;

namespace QuanLyCLB.Application.Entities;

public class PasswordResetOtp : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserAccountId { get; set; }

    public UserAccount User { get; set; } = null!;

    public string CodeHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public bool IsUsed { get; set; }
}
