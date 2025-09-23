using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuanLyClb.Domain.Entities;
using QuanLyClb.Domain.Enums;
using QuanLyClb.Infrastructure.Persistence;

namespace QuanLyClb.Infrastructure.Authorization;

public class DynamicAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<DynamicAuthorizationPolicyProvider> _logger;
    private const string RolePermissionsCacheKey = "DynamicAuthorizationPolicyProvider.RolePermissions";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public DynamicAuthorizationPolicyProvider(
        IOptions<AuthorizationOptions> options,
        IServiceScopeFactory scopeFactory,
        IMemoryCache memoryCache,
        ILogger<DynamicAuthorizationPolicyProvider> logger)
        : base(options)
    {
        _scopeFactory = scopeFactory;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var policy = await base.GetPolicyAsync(policyName);
        if (policy is not null)
        {
            return policy;
        }

        if (_memoryCache.TryGetValue(policyName, out AuthorizationPolicy cachedPolicy))
        {
            return cachedPolicy;
        }

        if (!TryParsePolicy(policyName, out var resource, out var action))
        {
            _logger.LogWarning("Invalid policy name {PolicyName}", policyName);
            return null;
        }

        var cachedPermissions = await _memoryCache.GetOrCreateAsync(
            RolePermissionsCacheKey,
            LoadRolePermissionsAsync);

        var roleNames = cachedPermissions
            .Where(permission => permission.HasPermission(resource, action))
            .Select(permission => permission.Role)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (roleNames.Length == 0)
        {
            _logger.LogWarning("No roles configured for policy {PolicyName}", policyName);
            return null;
        }

        policy = new AuthorizationPolicyBuilder()
            .RequireRole(roleNames)
            .Build();

        _memoryCache.Set(policyName, policy, TimeSpan.FromMinutes(10));
        return policy;
    }

    private async Task<IReadOnlyList<CachedRolePermission>> LoadRolePermissionsAsync(ICacheEntry cacheEntry)
    {
        cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var rolePermissions = await dbContext.RolePermissions
            .AsNoTracking()
            .ToListAsync();

        var cachedPermissions = new List<CachedRolePermission>(rolePermissions.Count);

        foreach (var rolePermission in rolePermissions)
        {
            if (TryDeserialize(rolePermission, out var permissions))
            {
                cachedPermissions.Add(new CachedRolePermission(rolePermission.Role.ToString(), permissions));
            }
            else
            {
                _logger.LogWarning("Role {Role} has no valid permission configuration", rolePermission.Role);
            }
        }

        return cachedPermissions;
    }

    private static bool TryParsePolicy(string policyName, out string resource, out PermissionAction action)
    {
        resource = string.Empty;
        action = default;

        var parts = policyName.Split(':', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            return false;
        }

        resource = parts[0];
        return Enum.TryParse(parts[1], ignoreCase: true, out action);
    }

    private bool TryDeserialize(RolePermission rolePermission, out IReadOnlyDictionary<string, HashSet<PermissionAction>> permissions)
    {
        permissions = new Dictionary<string, HashSet<PermissionAction>>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(rolePermission.PermissionsJson))
        {
            return false;
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<Dictionary<string, PermissionAction[]>>(rolePermission.PermissionsJson, JsonOptions);
            if (parsed is null)
            {
                return false;
            }

            foreach (var (resource, actions) in parsed)
            {
                if (string.IsNullOrWhiteSpace(resource) || actions is null || actions.Length == 0)
                {
                    continue;
                }

                permissions[resource] = new HashSet<PermissionAction>(actions);
            }

            return permissions.Count > 0;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse permissions json for role {Role}", rolePermission.Role);
            return false;
        }
    }

    private sealed record CachedRolePermission(string Role, IReadOnlyDictionary<string, HashSet<PermissionAction>> Permissions)
    {
        public bool HasPermission(string resource, PermissionAction action)
        {
            if (Permissions.TryGetValue(resource, out var actions))
            {
                return actions.Contains(action);
            }

            return false;
        }
    }
}
