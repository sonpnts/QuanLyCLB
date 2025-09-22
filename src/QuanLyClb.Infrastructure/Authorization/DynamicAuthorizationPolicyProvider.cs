using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuanLyClb.Infrastructure.Persistence;

namespace QuanLyClb.Infrastructure.Authorization;

public class DynamicAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<DynamicAuthorizationPolicyProvider> _logger;

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

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var roleNames = await dbContext.RolePermissions
            .Where(rp => rp.PolicyName == policyName)
            .Select(rp => rp.Role.ToString())
            .ToListAsync();

        if (roleNames.Count == 0)
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
}
