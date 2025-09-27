namespace QuanLyCLB.Application.Entities;

public class UserAccount
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string? GoogleSubject { get; set; }

    public string? PasswordHash { get; set; }

    public DateOnly CreatedDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public TimeOnly CreatedTime { get; set; } = TimeOnly.FromDateTime(DateTime.UtcNow);


    public bool IsActive { get; set; } = true;

    public Instructor? Instructor { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
