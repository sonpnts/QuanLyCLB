using System.Security.Claims;

namespace QuanLyCLB.Application.Interfaces;

public interface IJwtTokenService
{
    string CreateToken(IEnumerable<Claim> claims, DateTime expiresAtUtc);
}
