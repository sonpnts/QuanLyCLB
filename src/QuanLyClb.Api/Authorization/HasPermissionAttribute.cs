using Microsoft.AspNetCore.Authorization;
using QuanLyClb.Domain.Enums;

namespace QuanLyClb.Api.Authorization;

public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(Permission permission)
    {
        Policy = permission.ToString();
    }
}
