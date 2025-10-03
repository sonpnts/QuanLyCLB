using QuanLyCLB.Application.DTOs;

namespace QuanLyCLB.Application.Interfaces;

public interface IAuthService
{
    Task<InstructorAuthResult> AuthenticateWithCredentialsAsync(string username, string password, CancellationToken cancellationToken = default);

    (string Hash, string Salt) HashPassword(string password);
}
