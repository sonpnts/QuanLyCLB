using System;
using Microsoft.AspNetCore.Authorization;
using QuanLyClb.Domain.Enums;

namespace QuanLyClb.Api.Authorization;

public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string resource, PermissionAction action)
    {
        if (string.IsNullOrWhiteSpace(resource))
        {
            throw new ArgumentException("Resource name must be provided", nameof(resource));
        }

        Policy = $"{resource}:{action}";
    }
}
