using QuanLyClb.Domain.Enums;

namespace QuanLyClb.Domain.Entities;

public class RolePermission : BaseEntity
{
    public UserRole Role { get; set; }

    public string PermissionsJson { get; set; } = "{}";
}
