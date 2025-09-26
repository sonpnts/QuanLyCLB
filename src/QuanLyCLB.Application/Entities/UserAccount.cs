namespace QuanLyCLB.Application.Entities;

public class UserAccount
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string? GoogleSubject { get; set; }

    public bool IsActive { get; set; } = true;

    public Instructor? Instructor { get; set; }
}
