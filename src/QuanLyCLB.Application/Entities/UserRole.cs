namespace QuanLyCLB.Application.Entities;

public class UserRole
{
    public Guid UserAccountId { get; set; }

    public UserAccount User { get; set; } = null!;

    public Guid RoleId { get; set; }

    public Role Role { get; set; } = null!;
}
