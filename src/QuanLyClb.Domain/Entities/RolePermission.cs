using QuanLyClb.Domain.Enums;

namespace QuanLyClb.Domain.Entities;

public class RolePermission : BaseEntity
{
    public string PolicyName { get; set; } = string.Empty;

    public UserRole Role { get; set; }
}
