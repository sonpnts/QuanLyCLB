using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using QuanLyClb.Domain.Authorization;
using QuanLyClb.Domain.Entities;
using QuanLyClb.Infrastructure.Persistence;

namespace QuanLyClb.Api.Authorization;

public static class PermissionSeeder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static async Task SeedAsync(ApplicationDbContext dbContext)
    {
        var existingPermissions = await dbContext.RolePermissions
            .ToDictionaryAsync(rp => rp.Role);

        foreach (var mapping in PermissionRoleDefaults.DefaultRolePermissions)
        {
            var role = mapping.Key;
            var permissions = mapping.Value
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Distinct().ToArray());
            var permissionsJson = JsonSerializer.Serialize(permissions, JsonOptions);

            if (existingPermissions.TryGetValue(role, out var existing))
            {
                if (!string.Equals(existing.PermissionsJson, permissionsJson, StringComparison.Ordinal))
                {
                    existing.PermissionsJson = permissionsJson;
                    existing.UpdatedAt = DateTime.UtcNow;
                    dbContext.RolePermissions.Update(existing);
                }
            }
            else
            {
                dbContext.RolePermissions.Add(new RolePermission
                {
                    Role = role,
                    PermissionsJson = permissionsJson
                });
            }
        }

        if (dbContext.ChangeTracker.HasChanges())
        {
            await dbContext.SaveChangesAsync();
        }
    }
}
