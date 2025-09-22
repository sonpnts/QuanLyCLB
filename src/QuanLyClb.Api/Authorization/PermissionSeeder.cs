using System.Linq;
using Microsoft.EntityFrameworkCore;
using QuanLyClb.Domain.Entities;
using QuanLyClb.Infrastructure.Persistence;

namespace QuanLyClb.Api.Authorization;

public static class PermissionSeeder
{
    public static async Task SeedAsync(ApplicationDbContext dbContext)
    {
        foreach (var mapping in AuthorizationPolicies.DefaultRoleMappings)
        {
            var policyName = mapping.Key;
            var roles = mapping.Value;

            var existingRoles = await dbContext.RolePermissions
                .Where(rp => rp.PolicyName == policyName)
                .Select(rp => rp.Role)
                .ToListAsync();

            foreach (var role in roles)
            {
                if (!existingRoles.Contains(role))
                {
                    dbContext.RolePermissions.Add(new RolePermission
                    {
                        PolicyName = policyName,
                        Role = role
                    });
                }
            }
        }

        if (dbContext.ChangeTracker.HasChanges())
        {
            await dbContext.SaveChangesAsync();
        }
    }
}
